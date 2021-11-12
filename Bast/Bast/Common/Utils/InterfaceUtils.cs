namespace Bast.Common.Utils
{
    public class InterfaceUtils
    {
        public static string BuildBar(int length, int totalLength)
        {
            return new string (new string('|', length) + new string(' ', totalLength - length));
        }
    }
}
