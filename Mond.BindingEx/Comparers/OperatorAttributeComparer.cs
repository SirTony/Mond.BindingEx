using System.Collections.Generic;
using System.Reflection;

namespace Mond.BindingEx
{
    internal sealed class OperatorAttributeComparer : IEqualityComparer<MethodInfo>
    {
        public bool Equals( MethodInfo x, MethodInfo y )
        {
            var xOperator = x.GetCustomAttribute<MondOperatorAttribute>();
            var yOperator = y.GetCustomAttribute<MondOperatorAttribute>();

            return xOperator.Operator == yOperator.Operator;
        }

        public int GetHashCode( MethodInfo obj ) => obj.GetCustomAttribute<MondOperatorAttribute>()
                                                       .Operator.GetHashCode();
    }
}
