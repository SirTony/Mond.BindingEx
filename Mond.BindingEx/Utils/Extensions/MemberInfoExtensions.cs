using System.Reflection;

namespace Mond.BindingEx
{
    internal static class MemberInfoExtensions
    {
        public static string GetName( this MemberInfo info )
        {
            var alias = info.GetCustomAttribute<MondAliasAttribute>( true );
            return alias != null ? alias.Name : info.Name;
        }
    }
}
