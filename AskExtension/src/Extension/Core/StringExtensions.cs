namespace AskExtension.Core
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string text)
        {
            return text == null || text.Trim().Equals("");
        }
    }
}