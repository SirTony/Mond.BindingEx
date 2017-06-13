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
            LexerWrapper.LexerType = typeof( MondState ).GetTypeInfo()
                                                        .Assembly.GetTypes()
                                                        .First( t => t.FullName == "Mond.Compiler.Lexer" );

            LexerWrapper.IsOperatorTokenMethod = LexerWrapper.LexerType.GetMethod(
                "IsOperatorToken",
                new[] { typeof( string ) } );

            LexerWrapper.OperatorExistsMethod = LexerWrapper.LexerType.GetMethod(
                "OperatorExists",
                new[] { typeof( string ) } );
        }

        public static bool IsOperatorToken( string @operator )
            => (bool)LexerWrapper.IsOperatorTokenMethod.Invoke( null, new object[] { @operator } );

        public static bool OperatorExists( string @operator )
            => (bool)LexerWrapper.OperatorExistsMethod.Invoke( null, new object[] { @operator } );
    }
}
