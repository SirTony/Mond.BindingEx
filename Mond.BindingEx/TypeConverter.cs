using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mond.BindingEx
{
    internal static class TypeConverter
    {
        private static readonly ReadOnlyCollection<Type> numericTypes;
        private static readonly ReadOnlyCollection<Type> basicTypes;

        public static ReadOnlyCollection<Type> NumericTypes { get { return numericTypes; } }
        public static ReadOnlyCollection<Type> BasicTypes { get { return basicTypes; } }

        static TypeConverter()
        {
            numericTypes = new ReadOnlyCollection<Type>( new[]
            {
                typeof( sbyte ), typeof( byte ),
                typeof( short ), typeof( ushort ),
                typeof( int ), typeof( uint ),
                typeof( long ), typeof( ulong ),
                typeof( float ), typeof( double ),
            } );

            basicTypes = numericTypes.Concat( new[] { typeof( char ), typeof( string ), typeof( bool ) } ).ToList().AsReadOnly();
        }

        public static bool MatchTypes( MondValue[] values, Type[] types )
        {
            if( values.Length != types.Length )
                return false;

            return values.Zip( types, ( a, b ) => new { Value = a, Type = b } ).All( x => MatchType( x.Value, x.Type ) );
        }

        public static bool MatchTypes( MondValueType[] mondTypes, Type[] clrTypes )
        {
            if( mondTypes.Length != clrTypes.Length )
                return false;

            return mondTypes.Zip( clrTypes, ( a, b ) => new { MondType = a, ClrType = b } ).All( x => MatchType( x.MondType, x.ClrType ) );
        }

        public static bool MatchType( MondValue value, Type type )
        {
            if( type == typeof( char ) )
                return value.Type == MondValueType.String && value.ToString().Length == 1;

            return MatchType( value.Type, type );
        }

        public static bool MatchType( MondValueType mondType, Type clrType )
        {
            switch( mondType )
            {
                case MondValueType.False:
                case MondValueType.True:
                    return clrType == typeof( bool );

                case MondValueType.String:
                        return clrType == typeof( string );

                case MondValueType.Number:
                    return NumericTypes.Contains( clrType );

                case MondValueType.Null:
                case MondValueType.Undefined:
                    return !clrType.IsValueType;

                default:
                    UnsupportedMondTypeError( mondType );
                    break;
            }

            return false; // we should never get here
        }

        public static MondValue[] MarshalToMond( object[] values, MondValueType[] expectedTypes )
        {
            if( !MatchTypes( expectedTypes, values.Select( v => v.GetType() ).ToArray() ) )
                throw new ArgumentException( "Given values do not match expected types", "values" );

            return values.Zip( expectedTypes, ( a, b ) => new { Value = a, ExpectedType = b } ).Select( x => MarshalToMond( x.Value, x.ExpectedType ) ).ToArray();
        }

        public static MondValue MarshalToMond( object value, MondValueType expectedType )
        {
            if( !MatchType( expectedType, value.GetType() ) )
                throw new ArgumentException( "Given value does not match expected type", "value" );

            Console.WriteLine( value.GetType().Name );

            switch( expectedType )
            {
                case MondValueType.False:
                case MondValueType.True:
                    return (bool)value;

                case MondValueType.Null:
                    return MondValue.Null;

                case MondValueType.Undefined:
                    return MondValue.Undefined;

                case MondValueType.String:
                    return value.ToString();

                case MondValueType.Number:
                    return (double)Convert.ChangeType( value, typeof( double ) );

                default:
                    UnsupportedMondTypeError( expectedType );
                    break;
            }

            return null; // we should never get here
        }

        public static object[] MarshalToClr( MondValue[] values, Type[] expectedTypes )
        {
            if( !MatchTypes( values, expectedTypes ) )
                throw new ArgumentException( "Given values do not match expected types", "values" );

            return values.Zip( expectedTypes, ( a, b ) => new { Value = a, ExpectedType = b } ).Select( x => MarshalToClr( x.Value, x.ExpectedType ) ).ToArray();
        }

        public static object MarshalToClr( MondValue value, Type expectedType )
        {
            if( !MatchType( value, expectedType ) )
                throw new ArgumentException( "Given value does not match expected type", "value" );

            switch( value.Type )
            {
                case MondValueType.False:
                case MondValueType.True:
                    return (bool)value;

                case MondValueType.Null:
                case MondValueType.Undefined:
                    return null;

                case MondValueType.String:
                    return value.ToString();

                case MondValueType.Number:
                    return Convert.ChangeType( (double)value, expectedType );

                default:
                    UnsupportedMondTypeError( value.Type );
                    break;
            }

            return null; // we should never get here
        }

        public static MondValueType ToMondType( Type type )
        {
            if( NumericTypes.Contains( type ) )
                return MondValueType.Number;

            if( type == typeof( string ) || type == typeof( char ) )
                return MondValueType.String;

            if( type == typeof( bool ) )
                return MondValueType.True;

            UnsupportedClrTypeError( type );
            return MondValueType.Undefined; // we should never get here
        }

        //public static Type[] ToClrTypes( params MondValue[] values )
        //{
        //    if( values == null || values.Length == 0 )
        //        return Type.EmptyTypes;

        //    var types = new List<Type>( values.Length );

        //    foreach( var value in values )
        //    {
        //        switch( value.Type )
        //        {
        //            case MondValueType.False:
        //            case MondValueType.True:
        //                types.Add( typeof( bool ) );
        //                break;

        //            case MondValueType.Function:
        //                types.Add( typeof( MulticastDelegate ) );
        //                break;

        //            case MondValueType.Null:
        //            case MondValueType.Undefined:
        //                types.Add( typeof( object ) );
        //                break;

        //            case MondValueType.Number:
        //                types.Add( typeof( double ) );
        //                break;

        //            case MondValueType.Object:
        //                if( value.UserData != null )
        //                    types.Add( value.UserData.GetType() );
        //                else
        //                    goto default;

        //                break;

        //            case MondValueType.String:
        //                types.Add( typeof( string ) );
        //                break;

        //            default:
        //                UnsupportedTypeError( value.Type );
        //                break;
        //        }
        //    }

        //    return types.ToArray();
        //}

        private static void UnsupportedMondTypeError( MondValueType type )
        {
            throw new NotSupportedException( "Unsupported MondValueType '{0}'".With( type.GetName() ) );
        }

        private static void UnsupportedClrTypeError( Type type )
        {
            throw new NotSupportedException( "Unsupported CLR type '{0}'".With( type.Name ) );
        }
    }
}
