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

            if( value.Type == MondValueType.Object )
                return value.UserData != null && type.IsAssignableFrom( value.UserData.GetType() );

            return MatchType( value.Type, type );
        }

        public static bool MatchType( MondValueType mondType, Type clrType )
        {
            if( clrType == typeof( MondValue ) )
                return true;

            switch( mondType )
            {
                case MondValueType.False:
                case MondValueType.True:
                    return clrType == typeof( bool );

                case MondValueType.String:
                        return clrType == typeof( string );

                case MondValueType.Number:
                    return NumericTypes.Contains( clrType ) || clrType.IsEnum;

                case MondValueType.Null:
                case MondValueType.Undefined:
                    return !clrType.IsValueType;

                case MondValueType.Function:
                    return clrType == typeof( MulticastDelegate ) || clrType == typeof( Delegate )
                        || clrType.BaseType == typeof( MulticastDelegate ) || clrType.BaseType == typeof( Delegate );
                    
                case MondValueType.Object:
                    throw new NotSupportedException( "Object type matching is not supported by this overload of MatchType" );

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

            if( value is MondValue )
                return (MondValue)value;

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

                case MondValueType.Function:
                    if( value is MulticastDelegate || value.GetType().BaseType == typeof( MulticastDelegate ) )
                        return MondObjectBinder.Bind( value as MulticastDelegate );

                    if( value is Delegate || value.GetType().BaseType == typeof( Delegate ) )
                        return MondObjectBinder.Bind( value as Delegate );

                    throw new NotSupportedException( "Unsupported delegate type" );

                case MondValueType.Object:
                    return MondObjectBinder.Bind( value.GetType(), value, null, MondBindingOptions.AutoLock );

                default:
                    UnsupportedMondTypeError( expectedType );
                    break;
            }

            return null; // we should never get here
        }

        public static object[] MarshalToClr( MondValue[] values, Type[] expectedTypes, MondState state )
        {
            if( !MatchTypes( values, expectedTypes ) )
                throw new ArgumentException( "Given values do not match expected types", "values" );

            return values.Zip( expectedTypes, ( a, b ) => new { Value = a, ExpectedType = b } ).Select( x => MarshalToClr( x.Value, x.ExpectedType, state ) ).ToArray();
        }

        public static object MarshalToClr( MondValue value, Type expectedType, MondState state )
        {
            if( !MatchType( value, expectedType ) )
                throw new ArgumentException( "Given value does not match expected type", "value" );

            if( expectedType == typeof( MondValue ) )
                return value;

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
                    if( expectedType.IsEnum )
                    {
                        var underlying = Enum.GetUnderlyingType( expectedType );
                        var rawValue = Convert.ChangeType( (double)value, underlying );
                        var valueName = Enum.GetName( expectedType, rawValue );

                        return Enum.Parse( expectedType, valueName );
                    }

                    return Convert.ChangeType( (double)value, expectedType );

                case MondValueType.Object:
                    return value.UserData;

                case MondValueType.Function:
                    Func<object[], object> shim = delegate( object[] args )
                    {
                        var result = null as MondValue;

                        if( args == null )
                            result = state.Call( value );
                        else
                        {
                            var mondTypes = ToMondTypes( args.Select( a => a.GetType() ).ToArray() );
                            var mondValues = MarshalToMond( args, mondTypes );
                            result = state.Call( value, mondValues );
                        }

                        var clrType = ToClrType( result );
                        return MarshalToClr( result, clrType, state );
                    };

                    return shim;

                default:
                    UnsupportedMondTypeError( value.Type );
                    break;
            }

            return null; // we should never get here
        }

        public static MondValueType[] ToMondTypes( Type[] types )
        {
            return types.Select( ToMondType ).ToArray();
        }

        public static MondValueType ToMondType( Type type )
        {
            if( NumericTypes.Contains( type ) )
                return MondValueType.Number;

            if( type == typeof( string ) || type == typeof( char ) )
                return MondValueType.String;

            if( type == typeof( bool ) )
                return MondValueType.True;

            if( type == typeof( MulticastDelegate ) || type == typeof( Delegate ) )
                return MondValueType.Function;

            if( type.IsClass )
                return MondValueType.Object;

            UnsupportedClrTypeError( type );
            return MondValueType.Undefined; // we should never get here
        }

        public static Type[] ToClrTypes( MondValue[] values )
        {
            return values.Select( ToClrType ).ToArray();
        }

        public static Type ToClrType( MondValue value )
        {
            switch( value.Type )
            {
                case MondValueType.False:
                case MondValueType.True:
                    return typeof( bool );

                case MondValueType.Function:
                    return typeof( MulticastDelegate );

                case MondValueType.Number:
                    return typeof( double );

                case MondValueType.Object:
                    if( value.UserData != null )
                        return value.UserData.GetType();

                    goto default;

                case MondValueType.String:
                    return typeof( string );

                default:
                    UnsupportedMondTypeError( value.Type );
                    return null; // we should never get here
            }
        }

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
