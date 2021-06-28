using Bast.Main;
using System.Collections.Generic;
using System.Linq;

namespace Bast
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> argsList = args.ToList();
            IBastInterface bastInterface = new BastInterface(argsList);
            bastInterface.Init();
        }
    }
}
