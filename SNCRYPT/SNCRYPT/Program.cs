using SNCRYPT.Common;
using System;

namespace SNCRYPT
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            InteractionManager im = new InteractionManager(args);
            im.Start();
        }
    }
}
