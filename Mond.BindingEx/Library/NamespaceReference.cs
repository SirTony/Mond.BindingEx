using System;
using Mond.Binding;

namespace Mond.BindingEx.Library
{
    [MondClass]
    internal sealed class NamespaceReference
    {
        private readonly string _path;

        public NamespaceReference( string path )
        {
            if( string.IsNullOrWhiteSpace( path ) )
                throw new ArgumentNullException( nameof( path ) );

            this._path = path;
        }

        /// <summary>
        ///     Navigate namespaces or get a top-level type.
        /// </summary>
        [MondFunction( "__get" )]
        public MondValue Get( MondState state, MondValue instance, string name )
        {
            var newPath = this._path + "." + name;

            var type = InteropLibrary.LookupType( newPath );
            return type != null
                ? MondObjectBinder.Bind( type, state, MondBindingOptions.AutoLock )
                : new NamespaceReference( newPath ).ToMond( state );
        }

        /// <summary>
        ///     Binds type arguments to a generic type. Only used when the type does not have
        ///     an overload with no type arguments.
        /// </summary>
        [MondFunction( "__call" )]
        public MondValue Call( MondState state, MondValue instance, params MondValue[] args )
        {
            var types = InteropLibrary.GetTypeArray( args );

            var typeName = this._path + "`" + types.Length;
            var type = InteropLibrary.LookupType( typeName );
            if( type == null )
                throw new Exception( "Could not find type: " + typeName );

            var boundType = type.MakeGenericType( types );
            return MondObjectBinder.Bind( boundType, state, MondBindingOptions.AutoLock );
        }

        [MondFunction( "__string" )]
        public string String( MondValue instance ) => this._path;

        public MondValue ToMond( MondState state )
        {
            MondClassBinder.Bind<NamespaceReference>( state, out var prototype );

            var obj = new MondValue( state )
            {
                UserData = this,
                Prototype = prototype
            };
            obj.Lock();

            return obj;
        }
    }
}
