namespace Bast.Common.Types
{
    public class Data
    {
        public string S { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string Password { get; set; }

        public Data(string s, string name, int order, string password)
        {
            S = s;
            Name = name;
            Order = order;
            Password = password;
        }
    }
}
