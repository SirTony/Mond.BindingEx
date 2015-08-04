using System;

namespace Mond.BindingEx
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public sealed class MondOperatorAttribute : Attribute
    {
        private readonly string @operator;
        public string Operator { get { return this.@operator; } }

        public MondOperatorAttribute( string @operator )
        {
            if( !Utils.IsOperatorToken( @operator ) )
                throw new ArgumentException( "'{0}' is not a valid operator".With( @operator ), "operator" );

            if( Utils.OperatorExists( @operator ) )
                throw new ArgumentException( "Cannot override built-in operator '{0}'".With( @operator ), "operator" );

            this.@operator = @operator;
        }
    }
}
