using System;

namespace Mond.BindingEx
{
    internal struct ReflectedMember<T>
    {
        public bool Matched;
        public T Method;
        public Type[] Types;
    }
}
