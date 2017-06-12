using System.Collections.Generic;
using System.Reflection;
using Mond.BindingEx.Utils.Extensions;

namespace Mond.BindingEx.Comparers
{
    internal sealed class PropertyNameComparer : IEqualityComparer<PropertyInfo>
    {
        private readonly MondBindingOptions _options;

        public PropertyNameComparer( MondBindingOptions options ) { this._options = options; }

        public bool Equals( PropertyInfo x, PropertyInfo y ) => x.GetName( this._options ) ==
                                                                y.GetName( this._options );

        public int GetHashCode( PropertyInfo obj ) => obj.GetHashCode();
    }
}
