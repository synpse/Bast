using System.Collections.Generic;

namespace Bast.Encryption
{
    public interface IEncryptionManager
    {
        void FileDecrypt(List<string> inputFile, string password);
        void FileEncrypt(List<string> inputFile, string password);
    }
}