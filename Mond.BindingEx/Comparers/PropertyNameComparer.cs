using System.Collections.Generic;
using System.Reflection;

namespace Mond.BindingEx
{
    internal sealed class PropertyNameComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals( PropertyInfo x, PropertyInfo y ) => x.GetName() == y.GetName();

        public int GetHashCode( PropertyInfo obj ) => obj.GetHashCode();
    }
}
