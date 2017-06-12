using System;

namespace Mond.BindingEx
{
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class MondOperatorAttribute : Attribute
    {
        public string Operator { get; }

        public MondOperatorAttribute( string @operator )
        {
            if( !Utils.IsOperatorToken( @operator ) )
                throw new ArgumentException( "'{0}' is not a valid operator".With( @operator ), nameof( @operator ) );

            if( Utils.OperatorExists( @operator ) )
                throw new ArgumentException(
                    "Cannot override built-in operator '{0}'".With( @operator ),
                    nameof( @operator ) );

            this.Operator = @operator;
        }
    }
}
