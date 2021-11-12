namespace Bast.Common.Utils
{
    public class StringUtils
    {
        public static string ReplaceFirstOccurrence(string source, string target, string replacement)
        {
            int position = source.IndexOf(target);
            string result = source.Remove(position, target.Length).Insert(position, replacement);
            return result;
        }

        public static string ReplaceLastOccurrence(string source, string target, string replacement)
        {
            int position = source.LastIndexOf(target);
            string result = source.Remove(position, target.Length).Insert(position, replacement);
            return result;
        }
    }
}
