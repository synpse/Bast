using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

namespace SNCRYPT
{
    class Program
    {
        //  Call this function to remove the key from memory after use for security
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        [STAThread]
        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {
            Console.Clear();
            Console.Write("Encrypt, Decrypt or Quit? (E/D/Q): ");
            string str = Console.ReadLine();

            switch (str.ToUpperInvariant())
            {
                case "E":
                    FileOrFolderEncrypt();
                    break;

                case "D":
                    FileOrFolderDecrypt();
                    break;

                case "Q":
                    Environment.Exit(1);
                    break;

                default:
                    Start();
                    break;
            }

            Console.WriteLine("Done.");
            Thread.Sleep(1500);
            Start();
        }

        public static void FileOrFolderEncrypt()
        {
            Console.Clear();
            Console.Write("File, Directory or Back? (F/D/B): ");
            string str = Console.ReadLine();

            string password;
            string confirmPw;
            List<string> file;
            string folder;

            switch (str.ToUpperInvariant())
            {
                case "F":
                    AskFile(out file);

                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();
                    Console.Clear();

                    Console.Write("Set a password: ");
                    password = AskPassword();

                    Console.WriteLine("\n");

                    Console.Write("Confirm password: ");
                    confirmPw = AskPassword();

                    Console.Clear();

                    if (password == confirmPw)
                    {
                        FileEncrypt(file, password);
                    }
                    else
                    {
                        Console.WriteLine("Passwords do not match! Returning...");
                    }
                    break;

                case "D":
                    AskFolder(out folder);

                    file = DirRoutine(folder);

                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();
                    Console.Clear();

                    Console.Write("Set a password: ");
                    password = AskPassword();

                    Console.Write("\n");

                    Console.Write("Confirm password: ");
                    confirmPw = AskPassword();

                    Console.Clear();

                    if (password == confirmPw)
                    {
                        FileEncrypt(file, password);
                    }
                    else
                    {
                        Console.WriteLine("Passwords do not match! Returning...");
                    }
                    break;

                case "B":
                    Start();
                    break;

                default:
                    FileOrFolderEncrypt();
                    break;
            }
        }

        public static void FileOrFolderDecrypt()
        {
            Console.Clear();
            Console.Write("File, Directory or Back? (F/D/B): ");
            string str = Console.ReadLine();

            string password;
            List<string> file;
            string folder;

            switch (str.ToUpperInvariant())
            {
                case "F":
                    AskFile(out file);

                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();
                    Console.Clear();

                    Console.Write("Password: ");
                    password = AskPassword();

                    Console.Clear();

                    FileDecrypt(file, password);
                    break;

                case "D":
                    AskFolder(out folder);

                    file = DirRoutine(folder);

                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();
                    Console.Clear();

                    Console.Write("Password: ");
                    password = AskPassword();

                    Console.Clear();

                    FileDecrypt(file, password);
                    break;

                case "B":
                    Start();
                    break;

                default:
                    FileOrFolderDecrypt();
                    break;
            }
        }

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

        public static void AskFile(out List<string> file)
        {
            file = new List<string>();
            OpenFileDialog fd = new OpenFileDialog();

            if (fd.ShowDialog() == DialogResult.OK)
            {
                file.Add(fd.FileName);
                Console.WriteLine(file[0]);
            }
            else
            {
                Console.WriteLine("Invalid File. Returning...");
                Start();
            }
        }

        public static void AskFolder(out string folder)
        {
            folder = null;
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                folder = fbd.SelectedPath;
                Console.WriteLine(folder + " <=== START POINT");
            }
            else
            {
                Console.WriteLine("Invalid Folder. Returning...");
                Start();
            }
        }

        public static string AskPassword()
        {
            var password = string.Empty;
            ConsoleKey key;

            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password.Remove(password.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } 
            while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        /// <summary>
        /// Creates a random salt that will be used to encrypt your file. This method is required on FileEncrypt.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fill the buffer with the generated data
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        /// <summary>
        /// Encrypts a file from its path and a plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="password"></param>
        private static void FileEncrypt(List<string> inputFile, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            int i = 1;

            foreach (string s in inputFile)
            {
                //generate random salt
                byte[] salt = GenerateRandomSalt();

                //create output file name
                FileStream fsCrypt = new FileStream(s + ".locked", FileMode.Create);

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
                AES.Mode = CipherMode.CFB;

                // write salt to the begining of the output file, so in this case can be random every time
                fsCrypt.Write(salt, 0, salt.Length);

                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

                FileStream fsIn = new FileStream(s, FileMode.Open);

                //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
                byte[] buffer = new byte[1048576];
                int read;

                try
                {
                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                        cs.Write(buffer, 0, read);
                    }

                    // Close up
                    fsIn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Aborting! Error: " + ex);
                }
                finally
                {
                    cs.Close();
                    fsCrypt.Close();

                    File.Delete(s);
                    Console.WriteLine($"Encrypted file generated. Input file deleted. File {i} of {inputFile.Count}");
                    i++;
                }
            }

            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();

            Console.Write("Password wiped from memory. Current password pointer: ");

            password = null;

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("NULL");
            }
            else
            {
                Console.WriteLine(password);
            }

            Console.Write("Forcing GC... ");
            GC.Collect();
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="password"></param>
        private static void FileDecrypt(List<string> inputFile, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            int i = 1;

            foreach (string s in inputFile)
            {
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] salt = new byte[32];

                FileStream fsCrypt = new FileStream(s, FileMode.Open);
                fsCrypt.Read(salt, 0, salt.Length);

                RijndaelManaged AES = new RijndaelManaged();
                AES.KeySize = 256;
                AES.BlockSize = 128;
                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;

                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

                string outputFile = Path.ChangeExtension(s, null);

                FileStream fsOut = new FileStream(outputFile, FileMode.Create);

                int read;
                byte[] buffer = new byte[1048576];

                try
                {
                    while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Application.DoEvents();
                        fsOut.Write(buffer, 0, read);
                    }
                }
                catch (CryptographicException ex_CryptographicException)
                {
                    Console.WriteLine("Password Incorrect! Aborting.", ex_CryptographicException);
                    fsOut.Close();
                    File.Delete(outputFile);
                    fsCrypt.Close();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Aborting! Error: " + ex);
                    fsOut.Close();
                    File.Delete(outputFile);
                    fsCrypt.Close();
                    return;
                }

                try
                {
                    Console.WriteLine("Password correct! Decrypting...");
                    cs.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error by closing CryptoStream: " + ex);
                }
                finally
                {
                    fsOut.Close();
                    cs.Close();
                    fsCrypt.Close();

                    File.Delete(s);
                    Console.WriteLine($"Input file deleted. Decrypted file generated. File {i} of {inputFile.Count}");
                    i++;
                }
            }

            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();

            password = null;

            Console.Write("Password wiped from memory. Current password pointer: ");

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("NULL");
            }
            else
            {
                Console.WriteLine(password);
            }

            Console.Write("Forcing GC... ");
            GC.Collect();
        }
    }
}
