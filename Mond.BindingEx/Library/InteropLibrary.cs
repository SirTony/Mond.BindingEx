using System;
using System.Collections.Generic;
using System.Linq;
using Mond.Libraries;

namespace Mond.BindingEx.Library
{
    public class InteropLibraries : IMondLibraryCollection
    {
        public IEnumerable<IMondLibrary> Create( MondState state )
        {
            yield return new InteropLibrary();
        }
    }

    public class InteropLibrary : IMondLibrary
    {
        public IEnumerable<KeyValuePair<string, MondValue>> GetDefinitions()
        {
            var importNamespace = new MondValue( (state, instance, args) => new NamespaceReference( args[0] ).ToMond( state ) );
            yield return new KeyValuePair<string, MondValue>( "importNamespace", importNamespace );
        }

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
                
                if( value.Type != MondValueType.Object || !(value.UserData is TypeReference) )
                    throw new ArgumentException( "Argument #{0} is not a CLR type".With( i - 1 ), "values" );

                types[i] = ((TypeReference)value.UserData).Type;
            }

            return types;
        }
    }
}
