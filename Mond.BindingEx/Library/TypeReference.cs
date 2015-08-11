using System;
using Mond.Binding;

namespace Mond.BindingEx.Library
{
    [MondClass]
    internal class TypeReference
    {
        public Type Type { get; private set; }

        public TypeReference( Type type )
        {
            if( type == null )
                throw new ArgumentNullException("type");

            Type = type;
        }

        /// <summary>
        /// Get a nested type.
        /// </summary>
        [MondFunction( "__get" )]
        public MondValue Get( MondState state, MondValue instance, string name )
        {
            var typeName = Type.FullName + "+" + name;

            var type = InteropLibrary.LookupType( typeName );
            if( type == null )
                throw new Exception( "Could not find type: " + typeName );

            return MondObjectBinder.Bind( type, state, MondBindingOptions.AutoLock );
        }
        
        /// <summary>
        /// Bind type arguments to a generic type. Only used when the type has an overload
        /// with no type arguments.
        /// </summary>
        [MondFunction( "__call" )]
        public MondValue Call( MondState state, MondValue instance, params MondValue[] args )
        {
            if( Type.IsGenericType && !Type.ContainsGenericParameters )
                throw new Exception( "Generic type is already bound: " + Type.FullName );

            var types = InteropLibrary.GetTypeArray( args );

            var typeName = Type.FullName + "`" + types.Length;
            var type = InteropLibrary.LookupType( typeName );
            if( type == null )
                throw new Exception( "Could not find type: " + typeName );

            var boundType = type.MakeGenericType( types );
            return MondObjectBinder.Bind( boundType, state, MondBindingOptions.AutoLock );
        }

        [MondFunction( "__string" )]
        public string String( MondValue instance )
        {
            return Type.FullName;
        }
    }
}
