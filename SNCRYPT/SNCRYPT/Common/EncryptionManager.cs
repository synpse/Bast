using SNCRYPT.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SNCRYPT.Common
{
    public class EncryptionManager
    {
        //  Call this function to remove the key from memory after use for security
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        /// <summary>
        /// Encrypts a file from its path and a plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="password"></param>
        public void FileEncrypt(List<string> inputFile, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            int i = 1;

            foreach (string s in inputFile)
            {
                //generate random salt
                byte[] salt = SecurityUtils.GenerateRandomSalt();

                //create output file name
                FileStream fsCrypt = new FileStream(s + "1", FileMode.Create);

                //convert password string to byte arrray
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

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

                FileStream fsIn = new FileStream(s, FileMode.Open);

                //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
                byte[] buffer = new byte[1048576];
                int read;

                float counter = 1f;
                float fsCount = fsIn.Length;

                int progress = 0;

                try
                {
                    Console.Clear();
                    Console.WriteLine($"File {i} of {inputFile.Count}...\n");
                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        cs.Write(buffer, 0, read);
                        progress = (int)(((float)(counter * (float)buffer.Length / fsCount)) * 100);
                        Console.SetCursorPosition(0, Console.CursorTop);

                        if (progress > 100) progress = 100;

                        Console.Write($"[{InterfaceUtils.BuildBar((int)(progress / 2), 50)}] => {progress}%");
                        counter++;
                    }

                    // Close up
                    fsIn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n\nAborting! Error: " + ex);
                }
                finally
                {
                    cs.Close();
                    fsCrypt.Close();

                    File.Delete(s);

                    File.Move(s + "1", s);
                    Console.WriteLine($"\n\nEncrypted file generated. Input file deleted. File {i} of {inputFile.Count}.");
                    i++;
                }
            }

            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
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

            int i = 1;

            foreach (string s in inputFile)
            {
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] salt = new byte[32];

                FileStream fsCrypt = new FileStream(s, FileMode.Open);
                float fsCount = fsCrypt.Length;
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

                string outputFile = s + "1";

                FileStream fsOut = new FileStream(outputFile, FileMode.Create);

                int read;
                byte[] buffer = new byte[1048576];

                float counter = 1f;

                int progress = 0;

                try
                {
                    Console.Clear();
                    Console.WriteLine($"File {i} of {inputFile.Count}...\n");
                    while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fsOut.Write(buffer, 0, read);
                        progress = (int)(((float)(counter * (float)buffer.Length / fsCount)) * 100);
                        Console.SetCursorPosition(0, Console.CursorTop);

                        if (progress > 100) progress = 100;

                        Console.Write($"[{InterfaceUtils.BuildBar((int)(progress / 2), 50)}] => {progress}%");
                        counter++;
                    }
                }
                catch (CryptographicException ex_CryptographicException)
                {
                    Console.WriteLine("\n\nPassword Incorrect! Aborting.", ex_CryptographicException);
                    fsOut.Close();
                    File.Delete(outputFile);
                    fsCrypt.Close();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n\nAborting! Error: " + ex);
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
                    throw new Exception("\n\nError by closing CryptoStream: " + ex);
                }
                finally
                {
                    fsOut.Close();
                    cs.Close();
                    fsCrypt.Close();

                    File.Delete(s);
                    File.Move(s + "1", s);
                    Console.WriteLine($"\n\nInput file deleted. Decrypted file generated. File {i} of {inputFile.Count}.");
                    i++;
                }
            }

            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();
        }
    }
}
