using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mond.BindingEx.Utils.Extensions
{
    internal static class StringExtensions
    {
        private static readonly Regex IdentifierRegex;

        static StringExtensions()
        {
            StringExtensions.IdentifierRegex = new Regex(
                "([A-Z]+(?=$|[A-Z][a-z]|[0-9])|[A-Z]?[a-z]+|[0-9]+)",
                RegexOptions.Compiled );
        }

        [SuppressMessage( "ReSharper", "ArrangeStaticMemberQualifier" )]
        public static string ChangeNameCase( this string name )
        {
            if( !StringExtensions.IdentifierRegex.IsMatch( name ) ) return name;

            var matches = StringExtensions.IdentifierRegex
                                          .Matches( name )
                                          .Cast<Match>()
                                          .Select(
                                              m => m.Value
                                                    .ToLower()
                                                    .Trim( '_' )
                                          )
                                          .ToArray();

            return matches.First() + String.Join( String.Empty, matches.Skip( 1 ).Select( s => s.ToUpperFirst() ) );
        }

        public static string ToUpperFirst( this string value )
            => $"{Char.ToUpperInvariant( value[0] )}{value.Substring( 1 )}";
    }
}
