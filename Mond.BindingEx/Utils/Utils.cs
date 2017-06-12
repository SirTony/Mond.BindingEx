using System;
using System.Linq;
using System.Reflection;

namespace Mond.BindingEx
{
    internal static class Utils
    {
        private static readonly Type LexerType;
        private static readonly MethodInfo IsOperatorTokenMethod;
        private static readonly MethodInfo OperatorExistsMethod;

        static Utils()
        {
            Utils.LexerType = typeof( MondState ).Assembly.GetTypes().First( t => t.FullName == "Mond.Compiler.Lexer" );
            Utils.IsOperatorTokenMethod = Utils.LexerType.GetMethod(
                "IsOperatorToken",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof( string ) },
                null );
            Utils.OperatorExistsMethod = Utils.LexerType.GetMethod(
                "OperatorExists",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof( string ) },
                null );
        }

        public static bool IsOperatorToken( string @operator ) => (bool)Utils.IsOperatorTokenMethod.Invoke(
            null,
            new object[] { @operator } );

        public static bool OperatorExists( string @operator ) => (bool)Utils.OperatorExistsMethod.Invoke(
            null,
            new object[] { @operator } );
    }
}
