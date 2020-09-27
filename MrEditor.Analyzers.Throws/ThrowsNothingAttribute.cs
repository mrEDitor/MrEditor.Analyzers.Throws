using System;

namespace MrEditor.Analyzers.Throws
{
    /// <summary>
    /// Marks symbol as non-throwing.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue
    )]
    public class ThrowsNothingAttribute : Attribute
    {
    }
}
