using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mond.BindingEx.Utils.Extensions;

namespace Mond.BindingEx
{
    internal static class TypeConverter
    {
        public static IReadOnlyList<Type> NumericTypes { get; }

        public static IReadOnlyList<Type> BasicTypes { get; }

        static TypeConverter()
        {
            TypeConverter.NumericTypes = new[]
            {
                typeof( sbyte ), typeof( byte ),
                typeof( short ), typeof( ushort ),
                typeof( int ), typeof( uint ),
                typeof( long ), typeof( ulong ),
                typeof( float ), typeof( double )
            };

            TypeConverter.BasicTypes = TypeConverter.NumericTypes
                                                    .Concat(
                                                        new[] { typeof( char ), typeof( string ), typeof( bool ) } )
                                                    .ToArray();
        }

        public static bool MatchTypes( MondValue[] values, Type[] types )
        {
            if( values.Length != types.Length )
                return false;

            return values.Zip( types, ( a, b ) => new { Value = a, Type = b } )
                         .All( x => TypeConverter.MatchType( x.Value, x.Type ) );
        }

        public static bool MatchTypes( MondValueType[] mondTypes, Type[] clrTypes )
        {
            if( mondTypes.Length != clrTypes.Length )
                return false;

            return mondTypes.Zip( clrTypes, ( a, b ) => new { MondType = a, ClrType = b } )
                            .All( x => TypeConverter.MatchType( x.MondType, x.ClrType ) );
        }

        public static bool MatchType( MondValue value, Type type )
        {
            if( type == typeof( char ) )
                return ( value.Type == MondValueType.String ) && ( value.ToString().Length == 1 );

            if( value.Type == MondValueType.Object )
            {
                return ( type == typeof( MondValue ) ) ||
                       ( ( value.UserData != null ) && type.IsInstanceOfType( value.UserData ) );
            }

            return TypeConverter.MatchType( value.Type, type );
        }

        public static bool MatchType( MondValueType mondType, Type clrType )
        {
            if( ( clrType == typeof( MondValue ) ) || ( clrType == typeof( object ) ) )
                return true;

            var info = clrType.GetTypeInfo();
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch( mondType )
            {
                case MondValueType.False:
                case MondValueType.True: return clrType == typeof( bool );

                case MondValueType.String: return clrType == typeof( string );

                case MondValueType.Number:

                    bool IsNumeric( Type type ) => TypeConverter.NumericTypes.Contains( type ) ||
                                                   type.GetTypeInfo().IsEnum;

                    return IsNumeric( clrType ) ||
                           ( clrType.IsConstructedGenericType &&
                             ( clrType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) &&
                             IsNumeric( clrType.GetGenericArguments()[0] ) );

                case MondValueType.Null:
                case MondValueType.Undefined:
                    return !info.IsValueType ||
                           ( clrType.IsConstructedGenericType &&
                             ( clrType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) );

                case MondValueType.Function: return typeof( Delegate ).IsAssignableFrom( clrType );

                case MondValueType.Object:
                    throw new NotSupportedException(
                        "Object type matching is not supported by this overload of MatchType" );

                default:
                    TypeConverter.UnsupportedMondTypeError( mondType );
                    break;
            }

            return false; // we should never get here
        }

        public static MondValue[] MarshalToMond(
            object[] values,
            MondValueType[] expectedTypes,
            MondState state,
            MondBindingOptions options )
        {
            if( !TypeConverter.MatchTypes( expectedTypes, values.Select( v => v.GetType() ).ToArray() ) )
                throw new ArgumentException( "Given values do not match expected types", nameof( values ) );

            return values.Zip( expectedTypes, ( a, b ) => new { Value = a, ExpectedType = b } )
                         .Select( x => TypeConverter.MarshalToMond( x.Value, x.ExpectedType, state, options ) )
                         .ToArray();
        }

        public static MondValue MarshalToMond(
            object value,
            MondValueType expectedType,
            MondState state,
            MondBindingOptions options )
        {
            if( value == null ) return MondValue.Null;

            if( expectedType == MondValueType.Object )
            {
                if( state == null )
                {
                    throw new ArgumentNullException(
                        nameof( state ),
                        "Must have a valid MondState when binding an object" );
                }

                return MondObjectBinder.Bind( value.GetType(), value, state, options );
            }

            if( !TypeConverter.MatchType( expectedType, value.GetType() ) )
                throw new ArgumentException( "Given value does not match expected type", nameof( value ) );

            if( value is MondValue mond )
                return mond;

            var type = value.GetType();
            if( type.IsConstructedGenericType && ( type.GetGenericTypeDefinition() == typeof( Nullable<> ) ) )
            {
                var innerType = type.GetGenericArguments()[0];
                dynamic nullable = value;
                if( !TypeConverter.MatchType( expectedType, innerType ) ||
                    ( !nullable.HasValue &&
                      ( expectedType != MondValueType.Null ) &&
                      ( expectedType != MondValueType.Undefined ) ) )
                    throw new ArgumentException( "Given value does not match expected type", nameof( value ) );

                value = nullable.Value;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch( expectedType )
            {
                case MondValueType.False:
                case MondValueType.True: return (bool)value;

                case MondValueType.Null: return MondValue.Null;

                case MondValueType.Undefined: return MondValue.Undefined;

                case MondValueType.String: return value.ToString();

                case MondValueType.Number: return (double)Convert.ChangeType( value, typeof( double ) );

                case MondValueType.Function:
                    var info = value.GetType().GetTypeInfo();
                    if( value is MulticastDelegate || ( info.BaseType == typeof( MulticastDelegate ) ) )
                        return MondObjectBinder.Bind( value as MulticastDelegate );

                    if( value is Delegate || ( info.BaseType == typeof( Delegate ) ) )
                        return MondObjectBinder.Bind( value as Delegate );

                    throw new NotSupportedException( "Unsupported delegate type" );

                case MondValueType.Object:
                    return MondObjectBinder.Bind( value.GetType(), value, null, MondBindingOptions.AutoLock );

                default:
                    TypeConverter.UnsupportedMondTypeError( expectedType );
                    break;
            }

            return null; // we should never get here
        }

        public static object[] MarshalToClr(
            MondValue[] values,
            Type[] expectedTypes,
            MondState state,
            MondBindingOptions options )
        {
            if( !TypeConverter.MatchTypes( values, expectedTypes ) )
                throw new ArgumentException( "Given values do not match expected types", nameof( values ) );

            return values.Zip( expectedTypes, ( a, b ) => new { Value = a, ExpectedType = b } )
                         .Select( x => TypeConverter.MarshalToClr( x.Value, x.ExpectedType, state, options ) )
                         .ToArray();
        }

        public static object MarshalToClr(
            MondValue value,
            Type expectedType,
            MondState state,
            MondBindingOptions options )
        {
            if( !TypeConverter.MatchType( value, expectedType ) )
                throw new ArgumentException( "Given value does not match expected type", nameof( value ) );

            if( expectedType == typeof( MondValue ) )
                return value;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch( value.Type )
            {
                case MondValueType.False:
                case MondValueType.True: return (bool)value;

                case MondValueType.Null:
                case MondValueType.Undefined:
                    if( !expectedType.GetTypeInfo().IsValueType ) return null;

                    if( expectedType.IsConstructedGenericType &&
                        ( expectedType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) )
                        return Activator.CreateInstance( expectedType );

                    throw new InvalidOperationException(
                        $"Cannot bind {value.Type.GetName()} value to " +
                        $"{expectedType.FullName} because it is a value type" );

                case MondValueType.String:
                    var str = value.ToString();
                    if( expectedType != typeof( char ) ) return str;

                    if( str.Length != 1 )
                        throw new ArgumentException( "Value cannot be converted to char", nameof( value ) );

                    return str[0];

                case MondValueType.Number:
                    if( expectedType.IsConstructedGenericType &&
                        ( expectedType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) )
                    {
                        var innerType = expectedType.GetGenericArguments()[0];
                        var converted = Convert.ChangeType( (double)value, innerType );
                        return Activator.CreateInstance( expectedType, converted );
                    }

                    if( !expectedType.GetTypeInfo().IsEnum ) return Convert.ChangeType( (double)value, expectedType );

                    var underlying = Enum.GetUnderlyingType( expectedType );
                    var rawValue = Convert.ChangeType( (double)value, underlying );
                    var valueName = Enum.GetName( expectedType, rawValue );

                    // ReSharper disable once AssignNullToNotNullAttribute
                    return Enum.Parse( expectedType, valueName );

                case MondValueType.Object: return value.UserData;

                case MondValueType.Function:

                    object Shim( object[] args )
                    {
                        MondValue result;

                        if( ( args == null ) || ( args.Length == 0 ) )
                            result = state.Call( value );
                        else
                        {
                            var mondTypes = TypeConverter.ToMondTypes(
                                args.Select( a => a.GetType() ).ToArray() );
                            var mondValues = TypeConverter.MarshalToMond( args, mondTypes, state, options );
                            result = state.Call( value, mondValues );
                        }

                        if( ( result.Type == MondValueType.Null ) || ( result.Type == MondValueType.Undefined ) )
                            return null;

                        var clrType = TypeConverter.ToClrType( result );
                        return TypeConverter.MarshalToClr( result, clrType, state, options );
                    }

                    if( !typeof( Delegate ).IsAssignableFrom( expectedType ) ) return (Func<object[], object>)Shim;

                    var invoke = expectedType.GetMethod( "Invoke", BindingFlags.Public | BindingFlags.Instance );
                    var parameters = invoke.GetParameters().Select( p => p.ParameterType ).ToArray();
                    var delegateType = invoke.ReturnType == typeof( void )
                        ? Expression.GetActionType( parameters )
                        : Expression.GetFuncType( parameters.Concat( invoke.ReturnType ).ToArray() );

                    var shim = (Func<object[], object>)Shim;

                    var paramsExpr = parameters.Select( Expression.Parameter ).ToArray();
                    // ReSharper disable once CoVariantArrayConversion
                    var paramsArr = Expression.NewArrayInit( typeof( object ), paramsExpr );
                    var invokeExpr = Expression.Call(
                        Expression.Constant( shim.Target ),
                        shim.GetMethodInfo(),
                        paramsArr );

                    // ReSharper disable once TooWideLocalVariableScope
                    BlockExpression body;
                    if( invoke.ReturnType == typeof( void ) )
                        body = Expression.Block( invokeExpr );
                    else
                    {
                        var castExpr = Expression.Convert( invokeExpr, invoke.ReturnType );
                        body = Expression.Block( castExpr );
                    }

                    var method = typeof( Expression )
                            .GetMethods( BindingFlags.Public | BindingFlags.Static )
                            .Where( m => m.Name == "Lambda" )
                            .First( m => m.IsGenericMethodDefinition );

                    var lambda = method.MakeGenericMethod( delegateType );
                    return ( (LambdaExpression)lambda.Invoke( null, new object[] { body, paramsExpr } ) ).Compile();

                default:
                    TypeConverter.UnsupportedMondTypeError( value.Type );
                    break;
            }

            return null; // we should never get here
        }

        public static MondValueType[] ToMondTypes( Type[] types ) => types.Select( TypeConverter.ToMondType ).ToArray();

        public static MondValueType ToMondType( Type type )
        {
            if( TypeConverter.NumericTypes.Contains( type ) )
                return MondValueType.Number;

            if( ( type == typeof( string ) ) || ( type == typeof( char ) ) )
                return MondValueType.String;

            if( type == typeof( bool ) )
                return MondValueType.True;

            if( ( type == typeof( MulticastDelegate ) ) || ( type == typeof( Delegate ) ) )
                return MondValueType.Function;

            if( type.GetTypeInfo().IsClass )
                return MondValueType.Object;

            TypeConverter.UnsupportedClrTypeError( type );
            return MondValueType.Undefined; // we should never get here
        }

        public static Type[] ToClrTypes( MondValue[] values ) => values.Select( TypeConverter.ToClrType ).ToArray();

        public static Type ToClrType( MondValue value )
        {
            switch( value.Type )
            {
                case MondValueType.False:
                case MondValueType.True: return typeof( bool );

                case MondValueType.Function: return typeof( MulticastDelegate );

                case MondValueType.Number: return typeof( double );

                case MondValueType.Object:
                    if( value.UserData != null )
                        return value.UserData.GetType();

                    goto default;

                case MondValueType.String: return typeof( string );

                default:
                    TypeConverter.UnsupportedMondTypeError( value.Type );
                    return null; // we should never get here
            }
        }

        private static void UnsupportedMondTypeError( MondValueType type ) => throw new NotSupportedException(
            $"Unsupported MondValueType '{type.GetName()}'" );

        private static void UnsupportedClrTypeError( Type type ) => throw new NotSupportedException(
            $"Unsupported CLR type '{type.Name}'" );
    }
}
