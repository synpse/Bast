using System.Security;

namespace Bast.Common.Types
{
    public class Data
    {
        public string S { get; set; }
        public string Name { get; set; }
        public SecureString Password { get; set; }

        public Data(string s, string name, SecureString password)
        {
            S = s;
            Name = name;
            Password = password;
        }
    }
}
