using System.Collections.Generic;
using System.Reflection;
using Mond.BindingEx.Utils.Extensions;

namespace Mond.BindingEx.Comparers
{
    internal sealed class MethodNameComparer : IEqualityComparer<MethodInfo>
    {
        private readonly MondBindingOptions _options;

        public MethodNameComparer( MondBindingOptions options ) { this._options = options; }

        public bool Equals( MethodInfo x, MethodInfo y ) => x.GetName( this._options ) == y.GetName( this._options );

        public int GetHashCode( MethodInfo obj ) => obj.GetHashCode();
    }
}
