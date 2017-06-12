using System;
using System.Collections.Generic;
using System.Linq;
using Mond.Libraries;

namespace Mond.BindingEx.Library
{
    public class InteropLibraries : IMondLibraryCollection
    {
        public IEnumerable<IMondLibrary> Create( MondState state ) { yield return new InteropLibrary(); }
    }

    public class InteropLibrary : IMondLibrary
    {
        internal static Type LookupType( string typeName )
        {
            var type = Type.GetType( typeName );
            if( type != null )
                return type;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Select( a => a.GetType( typeName ) ).FirstOrDefault( t => t != null );
        }

        internal static Type[] GetTypeArray( MondValue[] values )
        {
            var types = new Type[values.Length];

            for( var i = 0; i < values.Length; ++i )
            {
                var value = values[i];

                if( ( value.Type != MondValueType.Object ) ||
                    ( !( value.UserData is TypeReference ) && !( value.UserData is Type ) ) )
                    throw new ArgumentException( $"Argument #{i} is not a CLR type", nameof( values ) );

                if( value.UserData is TypeReference typeRef )
                    types[i] = typeRef.Type;
                else
                    types[i] = (Type)value.UserData;
            }

            return types;
        }

        public IEnumerable<KeyValuePair<string, MondValue>> GetDefinitions()
        {
            var importNamespace = new MondValue(
                ( state, instance, args ) => new NamespaceReference( args[0] ).ToMond( state ) );
            yield return new KeyValuePair<string, MondValue>( "importNamespace", importNamespace );
        }
    }
}
