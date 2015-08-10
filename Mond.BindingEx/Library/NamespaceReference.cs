using System;
using Mond.Binding;

namespace Mond.BindingEx.Library
{
    [MondClass]
    internal class NamespaceReference
    {
        private readonly string _path;

        public NamespaceReference( string path )
        {
            if( string.IsNullOrWhiteSpace(path) )
                throw new ArgumentNullException( "path" );

            _path = path;
        }

        /// <summary>
        /// Navigate namespaces or get a top-level type.
        /// </summary>
        [MondFunction( "__get" )]
        public MondValue Get( MondState state, MondValue instance, string name )
        {
            var newPath = _path + "." + name;

            var type = InteropLibrary.LookupType( newPath );   
            if( type != null )
                return MondObjectBinder.Bind( type, state, MondBindingOptions.AutoLock );

            return new NamespaceReference( newPath ).ToMond( state );
        }

        /// <summary>
        /// Binds type arguments to a generic type. Only used when the type does not have
        /// an overload with no type arguments.
        /// </summary>
        [MondFunction( "__call" )]
        public MondValue Call( MondState state, MondValue instance, params MondValue[] args )
        {
            var types = InteropLibrary.GetTypeArray( args );

            var typeName = _path + "`" + types.Length;
            var type = InteropLibrary.LookupType( typeName );
            if( type == null )
                throw new Exception( "Could not find type: " + typeName );

            var boundType = type.MakeGenericType( types );
            return MondObjectBinder.Bind( boundType, state, MondBindingOptions.AutoLock );
        }

        [MondFunction( "__string" )]
        public string String( MondValue instance )
        {
            return _path;
        }

        public MondValue ToMond( MondState state )
        {
            MondValue prototype;
            MondClassBinder.Bind<NamespaceReference>( out prototype, state );

            var obj = new MondValue( state );
            obj.UserData = this;
            obj.Prototype = prototype;
            obj.Lock();

            return obj;
        }
    }
}
