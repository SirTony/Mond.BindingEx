using System;

namespace Mond.BindingEx
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public sealed class MondAliasAttribute : Attribute
    {
        public string Name { get; private set; }

        public MondAliasAttribute( string name )
        {
            if( String.IsNullOrWhiteSpace( name ) )
                throw new ArgumentException( "Name cannot be null, empty, or consist of only whitespace", "name" );

            this.Name = name;
        }
    }
}
