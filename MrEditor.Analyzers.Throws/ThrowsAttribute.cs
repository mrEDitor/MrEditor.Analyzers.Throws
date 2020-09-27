using System;

namespace MrEditor.Analyzers.Throws
{
    /// <summary>
    /// Marks symbol as throwing exceptions with any of <see cref="ThrowableTypes"/> or dervied.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue,
        AllowMultiple = true
    )]
    public class ThrowsAttribute : Attribute
    {
        /// <summary>
        /// Marks symbol as throwing exceptions with any of <paramref name="throwableTypes"/> or dervied.
        /// </summary>
        /// <param name="throwableTypes">List of exception types allowed to throw.</param>
        public ThrowsAttribute(params Type[] throwableTypes)
        {
            ThrowableTypes = throwableTypes ?? throw new ArgumentNullException(nameof(throwableTypes));
        }

        /// <summary>
        /// List of exception types that allowed to throw.
        /// </summary>
        public Type[] ThrowableTypes { get; }
    }
}
