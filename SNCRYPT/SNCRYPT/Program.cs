using Bast.Main;

namespace Bast
{
    class Program
    {
        static void Main(string[] args)
        {
            BastInitializer bastInitializer = new BastInitializer(args);
            bastInitializer.Init();
        }
    }
}
