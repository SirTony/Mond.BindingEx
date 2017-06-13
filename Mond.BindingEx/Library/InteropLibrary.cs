using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Mond.Libraries;

namespace Mond.BindingEx.Library
{
    public sealed class InteropLibraries : IMondLibraryCollection
    {
        public static InteropLibraries Instance { get; } = new InteropLibraries();

        private InteropLibraries() { }

        public IEnumerable<IMondLibrary> Create( MondState state ) { yield return new InteropLibrary(); }
    }

    public sealed class InteropLibrary : IMondLibrary
    {
        internal static Type LookupType( string typeName )
        {
            var type = Type.GetType( typeName );
            if( type != null )
                return type;

            return DependencyContext.Default
                                    .RuntimeLibraries
                                    .Select( lib => TryLoad( lib.Name ) )
                                    .Where( asm => asm != null )
                                    .SelectMany( asm => asm.GetTypes() )
                                    .Where( t => t.GetTypeInfo().IsPublic )
                                    .FirstOrDefault( t => t.FullName == typeName );

            Assembly TryLoad( string libraryName )
            {
                try
                {
                    var asmName = new AssemblyName( libraryName );
                    return Assembly.Load( asmName );
                }
                catch
                {
                    return null;
                }
            }
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
            MondValue ImportNamespace( MondState state, MondValue instance, MondValue[] args )
                => new NamespaceReference( args[0] ).ToMond( state );

            yield return new KeyValuePair<string, MondValue>( "using", new MondValue( ImportNamespace ) );
        }
    }
}
