using System;
using System.Collections.Generic;
using System.IO;

namespace Bast.Common.Utils
{
    public class DirectoryUtils
    {
        public static List<string> DirRoutine(string sDir)
        {
            List<string> paths = new List<string>();

            foreach (string f in Directory.GetFiles(sDir))
            {
                Console.WriteLine(f);
                paths.Add(f);
            }

            paths = DirSearch(sDir, paths);

            return paths;
        }

        public static List<string> DirSearch(string sDir, List<string> paths)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        Console.WriteLine(f);
                        paths.Add(f);
                    }

                    DirSearch(d, paths);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return paths;
        }
    }
}
