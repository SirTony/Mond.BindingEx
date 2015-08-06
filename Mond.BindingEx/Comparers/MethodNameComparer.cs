using System.Collections.Generic;
using System.Reflection;

namespace Mond.BindingEx
{
    internal sealed class MethodNameComparer : IEqualityComparer<MethodInfo>
    {
        public bool Equals( MethodInfo x, MethodInfo y )
        {
            return x.GetName() == y.GetName();
        }

        public int GetHashCode( MethodInfo obj )
        {
            return obj.GetHashCode();
        }
    }
}
