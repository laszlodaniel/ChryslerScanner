namespace ChryslerScanner.Helpers
{
    public static class StringExt
    {
        public static bool IsNumeric(this string text) => double.TryParse(text, out _);
    }
}
