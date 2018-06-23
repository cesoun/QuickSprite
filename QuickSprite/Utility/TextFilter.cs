using System.Text.RegularExpressions;

namespace QuickSprite.Utility
{
    public class TextFilter
    {
        private static readonly Regex _reg = new Regex("[^0-9-]+");
        public static bool NumericInput(string uinput) { return !_reg.IsMatch(uinput); }
    }
}
