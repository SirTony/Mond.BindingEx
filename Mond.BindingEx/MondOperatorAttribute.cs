using System;
using Mond.BindingEx.Utils;

namespace Mond.BindingEx
{
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class MondOperatorAttribute : Attribute
    {
        public string Operator { get; }

        public MondOperatorAttribute( string @operator )
        {
            if( !LexerWrapper.IsOperatorToken( @operator ) )
                throw new ArgumentException( $"'{@operator}' is not a valid operator", nameof( @operator ) );

            if( LexerWrapper.OperatorExists( @operator ) )
            {
                throw new ArgumentException(
                    $"Cannot override built-in operator '{@operator}'",
                    nameof( @operator ) );
            }

            this.Operator = @operator;
        }
    }
}
