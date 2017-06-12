using System;
using System.Linq;
using System.Reflection;

namespace Mond.BindingEx.Utils
{
    internal static class LexerWrapper
    {
        private static readonly Type LexerType;
        private static readonly MethodInfo IsOperatorTokenMethod;
        private static readonly MethodInfo OperatorExistsMethod;

        static LexerWrapper()
        {
            LexerWrapper.LexerType = typeof( MondState ).Assembly.GetTypes()
                                                        .First( t => t.FullName == "Mond.Compiler.Lexer" );
            LexerWrapper.IsOperatorTokenMethod = LexerWrapper.LexerType.GetMethod(
                "IsOperatorToken",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof( string ) },
                null );
            LexerWrapper.OperatorExistsMethod = LexerWrapper.LexerType.GetMethod(
                "OperatorExists",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof( string ) },
                null );
        }

        public static bool IsOperatorToken( string @operator )
            => (bool)LexerWrapper.IsOperatorTokenMethod.Invoke( null, new object[] { @operator } );

        public static bool OperatorExists( string @operator )
            => (bool)LexerWrapper.OperatorExistsMethod.Invoke( null, new object[] { @operator } );
    }
}
