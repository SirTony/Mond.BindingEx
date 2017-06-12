using System;

namespace Mond.BindingEx
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Method )]
    public sealed class MondIgnoreAttribute : Attribute { }
}
