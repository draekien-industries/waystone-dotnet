#if !NET5_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace System.Diagnostics
{
    [AttributeUsage(
        AttributeTargets.Method
      | AttributeTargets.Class
      | AttributeTargets.Constructor)]
    internal sealed class StackTraceHiddenAttribute : Attribute
    { }
}
#endif
