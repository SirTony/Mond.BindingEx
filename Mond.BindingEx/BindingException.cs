using System;

namespace Mond.BindingEx
{
    public sealed class BindingException : Exception
    {
        public Type BindingType { get; }
        public object Object { get; }

        internal BindingException( string message, Type bindingType, object obj )
            : base( message )
        {
            this.BindingType = bindingType;
            this.Object = obj;
        }
    }
}
