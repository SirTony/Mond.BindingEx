namespace Mond.BindingEx
{
    internal static class MondHelperMethods
    {
        public static bool EnumHasFlag( int enumValue, int flagValue )
        {
            return ( enumValue & flagValue ) == flagValue;
        }
    }
}
