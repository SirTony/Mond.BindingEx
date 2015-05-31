using System.Collections.Generic;
using System.Reflection;

namespace Mond.BindingEx
{
    internal sealed class PropertyNameComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals( PropertyInfo x, PropertyInfo y )
        {
            return x.Name == y.Name;
        }

        public int GetHashCode( PropertyInfo obj )
        {
            return obj.GetHashCode();
        }
    }
}
