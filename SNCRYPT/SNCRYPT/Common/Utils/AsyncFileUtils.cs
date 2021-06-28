using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bast.Common.Utils
{
    public class AsyncFileUtils
    {
        public static Task DeleteAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("Path is empty.");

            if (!File.Exists(path))
                throw new Exception("File does not exist.");

            return Task.Run(() => { File.Delete(path); });
        }

        public static Task<FileStream> CreateAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("Path is empty.");

            if (!File.Exists(path))
                throw new Exception("File does not exist.");

            return Task.Run(() => File.Create(path));
        }

        public static Task MoveAsync(string sourceFileName, string destFileName)
        {
            if (string.IsNullOrWhiteSpace(sourceFileName))
                throw new Exception("Path is empty.");

            if (string.IsNullOrWhiteSpace(destFileName))
                throw new Exception("Path is empty.");

            if (!File.Exists(sourceFileName))
                throw new Exception("File does not exist.");

            return Task.Run(() => { File.Move(sourceFileName, destFileName); });
        }
    }
}
