using System.Collections.Generic;
using System.Linq;

namespace Mond.BindingEx.Comparers
{
    internal class NumericTypeComparer<T> : IEqualityComparer<ReflectedMember<T>>
    {
        public bool Equals( ReflectedMember<T> x, ReflectedMember<T> y ) =>
                x.Types.All( TypeConverter.NumericTypes.Contains ) &&
                y.Types.All( TypeConverter.NumericTypes.Contains );

        public int GetHashCode( ReflectedMember<T> obj ) => obj.GetHashCode();
    }
}
