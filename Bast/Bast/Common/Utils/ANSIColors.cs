namespace Bast.Common.Utils
{
    public class ANSIColors
    {
        // From https://github.com/jsakamoto/Toolbelt.AnsiEscCode.Colorize/blob/master/Toolbelt.AnsiEscCode.Colorize/Colorize.cs
        // Original author jsakamoto

        private const string Default = "\x1b[0m";

        public static string DarkGray(string text) => "\x1b[90m" + text + Default;
        public static string Red(string text) => "\x1b[91m" + text + Default;
        public static string Green(string text) => "\x1b[92m" + text + Default;
        public static string Yellow(string text) => "\x1b[93m" + text + Default;
        public static string Blue(string text) => "\x1b[94m" + text + Default;
        public static string Magenta(string text) => "\x1b[95m" + text + Default;
        public static string Cyan(string text) => "\x1b[96m" + text + Default;
        public static string White(string text) => "\x1b[97m" + text + Default;
        public static string Black(string text) => "\x1b[30m" + text + Default;
        public static string DarkRed(string text) => "\x1b[31m" + text + Default;
        public static string DarkGreen(string text) => "\x1b[32m" + text + Default;
        //public static string DarkYellow(string text) => "\x1b[33m" + text + ColorEnd;
        public static string DarkYellow(string text) => "\x1b[38;2;193;156;0m" + text + Default;
        public static string DarkBlue(string text) => "\x1b[34m" + text + Default;
        //public static string DarkMagenta(string text) => "\x1b[35m" + text + ColorEnd;
        public static string DarkMagenta(string text) => "\x1b[38;2;136;23;152m" + text + Default;
        public static string DarkCyan(string text) => "\x1b[36m" + text + Default;
        public static string Gray(string text) => "\x1b[37m" + text + Default;
    }
}
