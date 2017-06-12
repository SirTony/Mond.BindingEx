using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mond.Binding;
using Mond.BindingEx.Comparers;
using Mond.BindingEx.Library;
using Mond.BindingEx.Utils;
using Mond.BindingEx.Utils.Extensions;

namespace Mond.BindingEx
{
    public static class MondObjectBinder
    {
        private static readonly Dictionary<Type, MondValue> BindingCache;

        static MondObjectBinder() { MondObjectBinder.BindingCache = new Dictionary<Type, MondValue>(); }

        public static MondValue Bind<T>( MondState state = null, MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind( typeof( T ), out var dummy, state, options );

        public static MondValue Bind<T>(
            out MondValue prototype,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind(
                typeof( T ),
                out prototype,
                state,
                options );

        public static MondValue Bind<T>(
            T instance,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind(
                typeof( T ),
                instance,
                state,
                options );

        public static MondValue Bind<T>(
            T instance,
            out MondValue prototype,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind(
                typeof( T ),
                instance,
                out prototype,
                state,
                options );

        public static MondValue Bind(
            Type type,
            object instance,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind( type, instance, out var dummy, state, options );

        public static MondValue Bind(
            Type type,
            object instance,
            out MondValue prototype,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
        {
            if( options.HasFlag( MondBindingOptions.AutoInsert ) )
            {
                throw new ArgumentException(
                    "MondBindingOptions.AutoInsert is not valid when binding object instances" );
            }

            var binding = new MondValue( state );
            MondObjectBinder.Bind( type, out prototype, state, options );

            binding.Prototype = prototype;
            binding.UserData = instance;

            return binding;
        }

        public static MondValue Bind(
            Type type,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind( type, out var dummy, state, options );

        public static MondValue Bind(
            MulticastDelegate function,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind(
                function as Delegate,
                state,
                options );

        public static MondValue Bind(
            Delegate function,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind(
                function,
                function.Method.GetName( options ),
                state,
                options );

        public static MondValue Bind(
            MulticastDelegate function,
            string name,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
            => MondObjectBinder.Bind(
                function as Delegate,
                name,
                state,
                options );

        public static MondValue Bind(
            Delegate function,
            string name,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
        {
            if( options.HasFlag( MondBindingOptions.AutoLock ) )
            {
                throw new ArgumentException(
                    "MondBindingOptions.AutoLock is not valid when binding delegates",
                    nameof( options ) );
            }

            var shim = BindingUtils.CreateStaticMethodShim( function, options );

            if( !options.HasFlag( MondBindingOptions.AutoInsert ) ) return shim;

            if( state == null )
            {
                throw new ArgumentNullException(
                    nameof( state ),
                    "Must specify a valid MondState when specifying MondBindingOptions.AutoInsert" );
            }

            if( String.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException(
                    "Must provide a valid name when specifying MondBindingOptions.AutoInsert",
                    nameof( name ) );
            }

            return state[name] = shim;
        }

        public static MondValue Bind(
            Type type,
            out MondValue prototype,
            MondState state = null,
            MondBindingOptions options = MondBindingOptions.None )
        {
            if( options.HasFlag( MondBindingOptions.AutoInsert ) && ( state == null ) )
            {
                throw new ArgumentNullException(
                    nameof( state ),
                    "A valid MondState must be given when MondBindingOptions.AutoInsert is present" );
            }

            prototype = null;

            var binding = null as MondValue;

            if( MondObjectBinder.BindingCache.ContainsKey( type ) )
            {
                binding = MondObjectBinder.BindingCache[type];
                prototype = binding.Prototype;

                return binding;
            }

            if( type.IsEnum )
                binding = MondObjectBinder.BindEnum( type, state, options );

            if( type.IsClass || type.IsStruct() )
                binding = MondObjectBinder.BindClass( type, state, out prototype, options );

            if( options.HasFlag( MondBindingOptions.AutoLock ) )
            {
                if( ( binding != null ) && ( binding.Type == MondValueType.Object ) )
                    binding.Lock();

                if( ( prototype != null ) && ( prototype.Type == MondValueType.Object ) )
                    prototype.Lock();
            }

            if( options.HasFlag( MondBindingOptions.AutoInsert ) )
                state[type.GetName()] = binding;

            MondObjectBinder.BindingCache.Add( type, binding );
            return binding;
        }

        private static MondValue BindEnum( Type type, MondState state, MondBindingOptions options )
        {
            var binding = new MondValue( state );
            var pairs = type.GetFields( BindingFlags.Public | BindingFlags.Static )
                            .Select( m => new { Name = m.GetName( options ), Value = m.GetRawConstantValue() } );

            foreach( var pair in pairs )
                binding[pair.Name] = (double)Convert.ChangeType( pair.Value, typeof( double ) );

            binding["hasFlag"] =
                    BindingUtils.CreateStaticMethodShim(
                        new Func<long, long, bool>( ( x, y ) => ( x & y ) == y ),
                        options );
            binding.UserData = type;
            return binding;
        }

        private static MondValue BindClass(
            Type type,
            MondState state,
            out MondValue prototype,
            MondBindingOptions options )
        {
            if( type.IsAbstract && !type.IsSealed )
                throw new ArgumentException( "Cannot bind abstract classes", nameof( type ) );

            if( type.IsInterface )
                throw new ArgumentException( "Cannot bind interfaces", nameof( type ) );

            prototype = new MondValue( state );
            var binding = new MondValue( state );
            var methodComparer = new MethodNameComparer( options );
            var propertyComparer = new PropertyNameComparer( options );
            var isStatic = type.IsSealed && type.IsAbstract;
            IEnumerable<MethodInfo> methods;
            IEnumerable<PropertyInfo> properties;

            if( isStatic )
                prototype = null;

            bool IsOperator( MethodInfo m ) => m.GetCustomAttribute<MondOperatorAttribute>() != null;

            bool IsProperty( MethodInfo m ) => m.IsSpecialName &&
                                               ( m.Name.StartsWith( "get_" ) || m.Name.StartsWith( "set_" ) );

            bool ShouldIgnore( MemberInfo m ) => ( m.GetCustomAttribute<MondIgnoreAttribute>() != null ) ||
                                                 ( m.GetCustomAttribute<CompilerGeneratedAttribute>() != null );

            if( !isStatic )
            {
                // Hook up instance methods
                methods = type.GetMethods( BindingFlags.Public | BindingFlags.Instance )
                              .Reject( IsOperator )
                              .Reject( IsProperty )
                              .Reject( ShouldIgnore )
                              .Distinct( methodComparer )
                              .ToArray();

                foreach( var method in methods )
                {
                    // Ignore some methods inherited from System.Object
                    if( /* method.Name == "ToString" || method.Name == "GetHashCode" || */
                        ( method.Name == "Equals" ) || ( method.Name == "GetType" ) )
                        continue;

                    var shim = BindingUtils.CreateInstanceMethodShim( type, method.GetName( options ), options );
                    prototype[method.GetName( options )] = shim;
                }

                if( methods.All( m => m.GetName( options ) != "__string" ) )
                {
                    var shim = BindingUtils.CreateInstanceMethodShim( type, "ToString", options );
                    prototype["__string"] = shim;
                }
            }

            // Hook up static methods
            methods = type.GetMethods( BindingFlags.Public | BindingFlags.Static )
                          .Reject( IsOperator )
                          .Reject( IsProperty )
                          .Reject( m => m.IsSpecialName && m.Name.StartsWith( "op_" ) )
                          .Reject( ShouldIgnore )
                          .Distinct( methodComparer )
                          .ToArray();

            foreach( var method in methods )
            {
                var shim = BindingUtils.CreateStaticMethodShim( type, method.GetName( options ), options );
                binding[method.GetName( options )] = shim;
            }

            // Hook up user defined operators
            methods = type.GetMethods( BindingFlags.Public | BindingFlags.Static )
                          .Where( IsOperator )
                          .Reject( ShouldIgnore )
                          .Distinct( new OperatorAttributeComparer() );

            if( !methods.Any() && ( state == null ) )
            {
                throw new ArgumentException(
                    "Must provide a valid MondState when attempting to bind user defined operators",
                    nameof( state ) );
            }

            foreach( var method in methods )
            {
                var attr = method.GetCustomAttribute<MondOperatorAttribute>();
                state["__ops"][attr.Operator] =
                        BindingUtils.CreateStaticMethodShim( type, method.GetName( options ), options );
            }

            if( !isStatic )
            {
                // Hook up instance properties
                properties = type.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                                 .Reject( ShouldIgnore )
                                 .Distinct( propertyComparer );

                foreach( var prop in properties )
                {
                    var propMethods = new[] { prop.GetGetMethod(), prop.GetSetMethod() }.Reject( m => m == null );
                    prototype[prop.GetName( options )] =
                            BindingUtils.CreateInstanceOverloadGroupShim( propMethods, options );
                }
            }

            // Hook up static properties
            properties = type.GetProperties( BindingFlags.Public | BindingFlags.Static )
                             .Reject( ShouldIgnore )
                             .Distinct( propertyComparer );

            foreach( var prop in properties )
            {
                var propMethods = new[] { prop.GetGetMethod(), prop.GetSetMethod() }.Reject( m => m == null );
                binding[prop.GetName( options )] = BindingUtils.CreateStaticOverloadGroupShim( propMethods, options );
            }

            if( !isStatic )
                    // Hook up the constructor
                binding["new"] = BindingUtils.CreateConstructorShim( type, prototype, options );

            MondClassBinder.Bind<TypeReference>( state, out var typePrototype );
            binding.Prototype = typePrototype;
            binding.UserData = new TypeReference( type );

            return binding;
        }
    }
}
