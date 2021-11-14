using Bast.Common.Enums;
using Bast.Common.Types;
using Bast.Common.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Threading;

namespace Bast.Operations.Encryption
{
    public class AESCBCEncryptionManager : IOperationManager
    {
        private int failedThreads;

        /// <summary>
        /// Launches an operation
        /// </summary>
        /// <param name="operationKind"></param>
        /// <param name="inputFile"></param>
        /// <param name="password"></param>
        public void LaunchOperation(OperationKind operationKind, List<string> inputFile, SecureString password)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Assume they all failed, each one will signal success and subtract
            failedThreads = inputFile.Count;
            List<Thread> threads = new List<Thread>();

            foreach (string s in inputFile)
            {
                string name = ANSIColors.DarkYellow(inputFile.IndexOf(s) + 1 + " | " + Guid.NewGuid().ToString());
                Data data = new Data(s, name, password);
                Thread thread = null;

                switch (operationKind)
                {
                    case OperationKind.Encrypt:
                        thread = new Thread(Encrypt)
                        {
                            Name = name
                        };
                        Console.WriteLine($"[{ANSIColors.Blue("BAST")}] Starting encryption on thread {thread.Name}");
                        break;

                    case OperationKind.Decrypt:
                        thread = new Thread(Decrypt)
                        {
                            Name = name
                        };
                        Console.WriteLine($"[{ANSIColors.Blue("BAST")}] Starting decryption on thread {thread.Name}");
                        break;
                }

                if (thread is null)
                {
                    return;
                }

                thread.Start(data);
                threads.Add(thread);
            }

            Thread[] threadArray = threads.ToArray();
            threads.Clear();

            while (threadArray.Length > 0)
            {
                for (int i = 0; i < threadArray.Length; i++)
                {
                    if (threadArray[i].Join(TimeSpan.Zero))
                    {
                        Console.WriteLine($"[{ANSIColors.Blue("BAST")}] {threadArray[i].Name} ended");
                        threadArray[i] = null;
                    }
                }

                threadArray = threadArray.Where(x => (x != null)).ToArray();
            }

            password.Clear();

            watch.Stop();

            Console.WriteLine($"[{ANSIColors.Blue("BAST")}] Operation completed for {inputFile.Count - failedThreads} of {inputFile.Count} files");

            if (failedThreads == 0)
            {
                Console.WriteLine($"[{ANSIColors.Blue("BAST")}] {ANSIColors.Green($"{failedThreads} operations failed")}");
            }
            else
            {
                Console.WriteLine($"[{ANSIColors.Blue("BAST")}] {ANSIColors.Red($"{failedThreads} operations failed")}");
            }

            Console.WriteLine($"[{ANSIColors.Blue("BAST")}] Took {watch.Elapsed.TotalSeconds} seconds");
        }

        private void Encrypt(object obj)
        {
            Data data = (Data)obj;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Generate random salt
            byte[] salt = SecurityUtils.GenerateRandomSalt();

            string tmpS;

            // Create temporary file
            if (data.S.Contains("\\"))
            {
                tmpS = StringUtils.ReplaceLastOccurrence(data.S, "\\", "\\.");
            }
            else
            {
                tmpS = "." + data.S;
            }

            byte[] bValue = SecurityUtils.SecureStringToByteArray(data.Password);

            FileStream fsCrypt = new FileStream(tmpS, FileMode.Create);

            // Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            // Repeatedly hash the user password along with the salt. 50000 iterations.
            var key = new Rfc2898DeriveBytes(bValue, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            // Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            AES.Mode = CipherMode.CBC;

            // Wipe password traces
            bValue = new byte[bValue.Length];
            bValue = null;
            GC.Collect();

            // Write salt to the begining of the output file, random every time
            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(data.S, FileMode.Open);

            // Create a 1mb buffer so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];
            int read;
            float counter = 1f;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cs.Write(buffer, 0, read);
                    counter++;
                }

                // Close up
                fsIn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("ENCRYPTING")}] {data.S} => {ANSIColors.Red("Aborting! Error:" + ex)}");
                File.Delete(tmpS);
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
            }

            // Write random bytes to file, prevent recovery
            using (FileStream fileStream = new FileStream(data.S, FileMode.Open))
            {
                Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("ENCRYPTING")}] {data.S} => Nuking old file with {fileStream.Length} bytes of data");

                // Create a 1mb buffer so only this amount will allocate in the memory and not the whole file
                buffer = new byte[1048576];
                counter = 1f;

                // Actual read-write operation
                try
                {
                    while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        new Random().NextBytes(buffer);
                        fileStream.Write(buffer, 0, read);
                        counter++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("ENCRYPTING")}] {data.S} => {ANSIColors.Red("Aborting! Error:" + ex)}");
                }
            }

            Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("ENCRYPTING")}] {data.S} => File destroyed. Deleting");

            // Safely delete the nuked file
            File.Delete(data.S);

            Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("ENCRYPTING")}] {data.S} => Renaming encrypted file");

            // Rename the encrypted file
            File.Move(tmpS, data.S);

            failedThreads--;

            watch.Stop();
            Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("ENCRYPTING")}] {data.S} => Done. Took {watch.Elapsed.TotalSeconds} seconds");
        }

        private void Decrypt(object obj)
        {
            Data data = (Data)obj;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            byte[] bValue = SecurityUtils.SecureStringToByteArray(data.Password);

            // Allocate salt buffer
            byte[] salt = new byte[32];

            // Open file stream read
            FileStream fsCrypt = new FileStream(data.S, FileMode.Open);

            // Read salt
            fsCrypt.Read(salt, 0, salt.Length);

            // Set Rijndael symmetric encryption algorithm and get data
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            // Repeatedly hash the user password along with the salt. 50000 iterations.
            var key = new Rfc2898DeriveBytes(bValue, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            // Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            AES.Mode = CipherMode.CBC;

            // Wipe password traces
            bValue = new byte[bValue.Length];
            bValue = null;
            GC.Collect();

            // Open crypto stream
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            string tmpS;

            // Create temporary file
            if (data.S.Contains("\\"))
            {
                tmpS = StringUtils.ReplaceLastOccurrence(data.S, "\\", "\\.");
            }
            else
            {
                tmpS = "." + data.S;
            }

            string outputFile = tmpS;

            // Open file stream write
            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            // Create a 1mb buffer so only this amount will allocate in the memory and not the whole file
            int read;
            byte[] buffer = new byte[1048576];
            float counter = 1f;

            // Actual read-write operation
            try
            {
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fsOut.Write(buffer, 0, read);
                    counter++;
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("DECRYPTING")}] {data.S} => { ANSIColors.Red("Password Incorrect! Aborting.")}", ex_CryptographicException);
                fsOut.Close();
                File.Delete(outputFile);
                fsCrypt.Close();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("DECRYPTING")}] {data.S} => {ANSIColors.Red("Aborting! Error:" + ex)}");
                fsOut.Close();
                File.Delete(outputFile);
                fsCrypt.Close();
                return;
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("DECRYPTING")}] {data.S} => {ANSIColors.Red("Aborting! Error closing CryptoStream:" + ex)}");
                File.Delete(tmpS);
            }
            finally
            {
                fsOut.Close();
                cs.Close();
                fsCrypt.Close();
            }

            Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("DECRYPTING")}] {data.S} => Deleting old encrypted file");

            // Delete old encrypted file (no need to nuke this one as it is encrypted and we just decrypted the actual file)
            File.Delete(data.S);

            Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("DECRYPTING")}] {data.S} => Renaming decrypted file");

            // Rename decrypted file
            File.Move(tmpS, data.S);

            failedThreads--;

            watch.Stop();
            Console.WriteLine($"[{data.Name}] [{ANSIColors.Green("DECRYPTING")}] {data.S} => Done. Took {watch.Elapsed.TotalSeconds} seconds");
        }
    }
}
