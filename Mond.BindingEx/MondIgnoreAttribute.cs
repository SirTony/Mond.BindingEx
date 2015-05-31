using System;

namespace Mond.BindingEx
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false )]
    public sealed class MondIgnoreAttribute : Attribute { }
}
