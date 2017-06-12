using System.Reflection;

namespace Mond.BindingEx.Utils.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static string GetName( this MemberInfo info, MondBindingOptions options )
        {
            var name = info.GetCustomAttribute<MondAliasAttribute>( true )?.Name ?? info.Name;
            return options.HasFlag( MondBindingOptions.PreserveNames ) && !name.StartsWith( "__" )
                ? name
                : name.ChangeNameCase();
        }
    }
}
