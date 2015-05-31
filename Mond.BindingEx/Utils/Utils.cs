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
            LexerType = typeof( MondState ).Assembly.GetTypes().First( t => t.FullName == "Mond.Compiler.Lexer" );
            IsOperatorTokenMethod = LexerType.GetMethod( "IsOperatorToken", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof( string ) }, null );
            OperatorExistsMethod = LexerType.GetMethod( "OperatorExists", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof( string ) }, null );
        }

        public static bool IsOperatorToken( string @operator )
        {
            return (bool)IsOperatorTokenMethod.Invoke( null, new object[] { @operator } );
        }

        public static bool OperatorExists( string @operator )
        {
            return (bool)OperatorExistsMethod.Invoke( null, new object[] { @operator } );
        }
    }
}
