using Bast.Common.Types;
using Bast.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace Bast.Encryption
{
    public class AESCBCEncryptionManager : IEncryptionManager
    {
        /// <summary>
        /// Encrypts a file from its path and a plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="password"></param>
        public void FileEncrypt(List<string> inputFile, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);
            List<Thread> threads = new List<Thread>();
            bool ended = false;
            int threadsEnded = 0;
            int lastThreadsEnded = 0;

            foreach (string s in inputFile)
            {
                string name = Guid.NewGuid().ToString();
                int order = inputFile.IndexOf(s) + 1;
                Data data = new Data(s, name, order, password);
                Thread thread = new Thread(Encrypt)
                {
                    Name = name
                };
                thread.Start(data);

                threads.Add(thread);
                Console.WriteLine($"[MASTER-THREAD] [ENCRYPTING] Started thread {thread.Name} with order {order}");
            }

            while (!ended)
            {
                foreach (Thread thread in threads)
                {
                    if (!thread.IsAlive)
                    {
                        threadsEnded++;
                    }

                    if (threadsEnded != lastThreadsEnded)
                        Console.WriteLine($"[MASTER-THREAD] [ENCRYPTING] {threadsEnded} threads ended of {inputFile.Count}...");

                    lastThreadsEnded = threadsEnded;

                    if (threadsEnded == inputFile.Count)
                    {
                        ended = true;
                        break;
                    }
                }
            }

            gch.Free();
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="password"></param>
        public void FileDecrypt(List<string> inputFile, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);
            List<Thread> threads = new List<Thread>();
            bool ended = false;
            int threadsEnded = 0;
            int lastThreadsEnded = 0;

            foreach (string s in inputFile)
            {
                string name = Guid.NewGuid().ToString();
                int order = inputFile.IndexOf(s) + 1;
                Data data = new Data(s, name, order, password);
                Thread thread = new Thread(Decrypt)
                {
                    Name = name
                };
                thread.Start(data);

                threads.Add(thread);
                Console.WriteLine($"[MASTER-THREAD] [DECRYPTING] Started thread {thread.Name} with order {order}");
            }

            while (!ended)
            {
                foreach (Thread thread in threads)
                {
                    if (!thread.IsAlive)
                    {
                        threadsEnded++;
                    }

                    if (threadsEnded != lastThreadsEnded)
                        Console.WriteLine($"[MASTER-THREAD] [DECRYPTING] {threadsEnded} threads ended of {inputFile.Count}...");

                    lastThreadsEnded = threadsEnded;

                    if (threadsEnded == inputFile.Count)
                    {
                        ended = true;
                        break;
                    }
                }
            }

            gch.Free();
        }

        private void Encrypt(object obj)
        {
            Data data = (Data)obj;

            //generate random salt
            byte[] salt = SecurityUtils.GenerateRandomSalt();

            string tmpS;

            if (data.S.Contains("\\"))
            {
                tmpS = StringUtils.ReplaceLastOccurrence(data.S, "\\", "\\.");
            }
            else
            {
                tmpS = "." + data.S;
            }

            FileStream fsCrypt = new FileStream(tmpS, FileMode.Create);

            //convert password string to byte arrray
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(data.Password);

            //Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            //Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            AES.Mode = CipherMode.CBC;

            // write salt to the begining of the output file, so in this case can be random every time
            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(data.S, FileMode.Open);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
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
                Console.WriteLine($"\u001b[31m[{data.Order} | {data.Name}] [ENCRYPTING] {data.S} => Aborting! Error: " + ex + "\u001b[0m");
                File.Delete(tmpS);
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
            }

            // Write random shit to file here here, prevent recovery
            using (FileStream fileStream = new FileStream(data.S, FileMode.Open))
            {
                long size = fileStream.Length;
                byte[] dataArray = new byte[size];
                new Random().NextBytes(dataArray);
                Console.WriteLine($"[{data.Order} | {data.Name}] [ENCRYPTING] {data.S} => Nuking old file with {size} bytes of data.");

                // Write the data to the file, byte by byte.
                for (int i = 0; i < dataArray.Length; i++)
                {
                    fileStream.WriteByte(dataArray[i]);
                }
            }

            File.Delete(data.S);
            File.Move(tmpS, data.S);
        }

        private void Decrypt(object obj)
        {
            Data data = (Data)obj;

            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(data.Password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(data.S, FileMode.Open);

            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            string tmpS;

            if (data.S.Contains("\\"))
            {
                tmpS = StringUtils.ReplaceLastOccurrence(data.S, "\\", "\\.");
            }
            else
            {
                tmpS = "." + data.S;
            }

            string outputFile = tmpS;

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];
            float counter = 1f;

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
                Console.WriteLine($"\u001b[31m[{data.Order} | {data.Name}] [DECRYPTING] {data.S} => Password Incorrect! Aborting." + "\u001b[0m", ex_CryptographicException);
                fsOut.Close();
                File.Delete(outputFile);
                fsCrypt.Close();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[{data.Order} | {data.Name}] [31m[DECRYPTING] {data.S} => Aborting! Error: " + ex + "\u001b[0m");
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
                Console.WriteLine($"\u001b[31m[{data.Order} | {data.Name}] [DECRYPTING] {data.S} => Error closing CryptoStream: " + ex + "\u001b[0m");
                File.Delete(tmpS);
            }
            finally
            {
                fsOut.Close();
                cs.Close();
                fsCrypt.Close();
            }

            File.Delete(data.S);
            File.Move(tmpS, data.S);
        }
    }
}
