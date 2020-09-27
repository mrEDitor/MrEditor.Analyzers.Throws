using Microsoft.CodeAnalysis;

namespace MrEditor.Analyzers.Throws.Analyzer.Rules
{
    public static class ExceptionsUsage
    {
        private const string Category = "Exceptions Usage";

        public static DiagnosticDescriptor ThrowOnlyDeclaredExceptionsRule { get; } = new DiagnosticDescriptor(
            "TRW001",
            (LocalizableString)"Method or property does not declare some exception types that may be thrown.",
            (LocalizableString)"Mark method with [Throws(typeof({0}))] attribute",
            Category,
            DiagnosticSeverity.Error,
            true
        );

        public static DiagnosticDescriptor DoNotThrowRule { get; } = new DiagnosticDescriptor(
            "TRW002",
            (LocalizableString)"Method or property declares [ThrowsNothing] but some exceptions may be thrown inside.",
            (LocalizableString)"Remove [ThrowsNothing] attribute",
            Category,
            DiagnosticSeverity.Error,
            true
        );

        public static DiagnosticDescriptor NonClsComplaintExceptionsNotSupportedYet { get; } = new DiagnosticDescriptor(
            "TRW901",
            (LocalizableString)"Catch clause rethrows non-CLS-exception (not supported by analyzer yet).",
            (LocalizableString)"Non-CLS-complaint exception rethrown here, but can not be handled by analyzer yet",
            Category,
            DiagnosticSeverity.Info,
            true
        );
    }
}
