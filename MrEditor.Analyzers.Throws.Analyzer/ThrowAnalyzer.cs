using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using MrEditor.Analyzers.Throws.Analyzer.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MrEditor.Analyzers.Throws.Analyzer
{
    /// <summary>
    /// Analyzer for <see langword="throw"/> expressions and statements.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThrowAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            ExceptionsUsage.ThrowOnlyDeclaredExceptionsRule,
            ExceptionsUsage.DoNotThrowRule,
            ExceptionsUsage.NonClsComplaintExceptionsNotSupportedYet
        );

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation, OperationKind.PropertyReference);
            context.RegisterOperationAction(AnalyzeThrow, OperationKind.Throw);
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            TryGetSymbolThrowableTypes(
                context.Compilation,
                context.ContainingSymbol,
                out var throwableTypes
            );

            if (TryGetSymbolThrowableTypes(
                context.Compilation,
                context.Operation switch
                {
                    IInvocationOperation invokeOp =>
                        invokeOp.TargetMethod,
                    IPropertyReferenceOperation propOp
                    when (propOp.Parent as IAssignmentOperation)?.Target == propOp =>
                        propOp.Property.SetMethod
                            ?? throw new InvalidOperationException($"Setter not provided for {context.Operation}"),
                    IPropertyReferenceOperation propOp =>
                        propOp.Property.GetMethod
                            ?? throw new InvalidOperationException($"Getter not provided for {context.Operation}"),
                    _ =>
                        throw new NotSupportedException($"Operation not supported: {context.Operation}"),
                },
                out var requiredThrowableTypes
            ))
            {
                foreach (var type in requiredThrowableTypes)
                {
                    AnalyzeOperation(context, throwableTypes, type);
                }
            }
        }

        private void AnalyzeThrow(OperationAnalysisContext context)
        {
            TryGetSymbolThrowableTypes(
                context.Compilation,
                context.ContainingSymbol,
                out var throwableTypes
            );
            AnalyzeOperation(
                context,
                throwableTypes,
                context.Operation.Children.SingleOrDefault() switch
                {
                    IConversionOperation convertion => convertion.Operand.Type,
                    IOperation operation => operation.Type,
                    null => GetParentCatch(context.Operation).ExceptionType,
                }
            );

            static ICatchClauseOperation GetParentCatch(IOperation operation)
            {
                for (var parentOp = operation; parentOp != null; parentOp = parentOp.Parent)
                {
                    if (parentOp is ICatchClauseOperation catchOp)
                    {
                        return catchOp;
                    }
                }

                // TODO: is it allowed to throw exceptions in case of incorrect syntax?
                // TODO: is there better exception types like with context argument?
                throw new InvalidOperationException("Unexpected `throw;` outside of `catch {}`.");
            }
        }

        private void AnalyzeOperation(
            OperationAnalysisContext context,
            IReadOnlyCollection<ITypeSymbol> allowedTypes,
            ITypeSymbol typeToThrow
        )
        {
            for (var parentOp = context.Operation; parentOp != null; parentOp = parentOp.Parent)
            {
                switch (parentOp)
                {
                    case ITryOperation tryOp:
                        allowedTypes = allowedTypes
                            .Concat(tryOp.Catches.Select(catchOp => catchOp.ExceptionType))
                            .ToList();
                        break;
                    case ICatchClauseOperation catchOp:
                        // skip it's parent of ITryOperation since
                        // it doesn't catch exceptions in own catch clauses
                        parentOp = parentOp.Parent;
                        break;
                }
            }

            for (var catchable = typeToThrow; catchable != null; catchable = catchable.BaseType)
            {
                if (allowedTypes.Contains(catchable))
                {
                    return;
                }
            }

            var objectType = context.Compilation.GetSpecialType(SpecialType.System_Object);
            context.ReportDiagnostic(Diagnostic.Create(
                0 switch
                {
                    _ when SymbolEqualityComparer.Default.Equals(typeToThrow, objectType) =>
                        ExceptionsUsage.NonClsComplaintExceptionsNotSupportedYet,
                    _ when allowedTypes.Any() =>
                        ExceptionsUsage.ThrowOnlyDeclaredExceptionsRule,
                    _ =>
                        ExceptionsUsage.DoNotThrowRule,
                },
                context.Operation.Syntax.GetLocation(),
                typeToThrow
            ));
        }

        private static bool TryGetSymbolThrowableTypes(
            Compilation compilation,
            ISymbol symbol,
            out IReadOnlyCollection<ITypeSymbol> throwableTypes
        )
        {
            if (GetAttributes<ThrowsNothingAttribute>().Any())
            {
                throwableTypes = Array.Empty<ITypeSymbol>();
                return false;
            }
            else
            {
                // TODO: Is IEnumerable annotated badly?
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                throwableTypes = GetAttributes<ThrowsAttribute>()
                    .SelectMany(attr => attr.ConstructorArguments)
                    .SelectMany(
                        // arguments may be not recognized as array during typing
                        arg => arg.Kind == TypedConstantKind.Array
                            ? arg.Values.Select(type => type.Value).OfType<ITypeSymbol>()
                            : Enumerable.Empty<ITypeSymbol>()
                    )
                    .DefaultIfEmpty(compilation.GetTypeByMetadataName(typeof(Exception).FullName))
                    .ToList();
                return true;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }

            IEnumerable<AttributeData> GetAttributes<T>()
            {
                var attrSymbol = compilation.GetTypeByMetadataName(typeof(T).FullName);
                return symbol.GetAttributes()
                    .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol));
            }
        }
    }
}
