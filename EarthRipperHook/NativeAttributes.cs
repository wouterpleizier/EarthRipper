namespace EarthRipperHook
{
    [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    internal class X86FunctionNameAttribute(string functionName) : Attribute
    {
        public string Name { get; } = functionName;
    }

    [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    internal class X64FunctionNameAttribute(string functionName) : Attribute
    {
        public string Name { get; } = functionName;
    }

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    internal class FunctionLibraryAttribute(string library) : Attribute
    {
        public string Library { get; } = library;
    }
}
