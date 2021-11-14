using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace Bast.Common.Utils
{
    public class SecurityUtils
    {
        public static SecureString AskPassword()
        {
            SecureString password = new SecureString();
            ConsoleKey key;

            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password.RemoveAt(password.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write(ANSIColors.White("*"));
                    password.AppendChar(keyInfo.KeyChar);
                }
            }
            while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        public static bool PasswordsMatch(SecureString ss1, SecureString ss2)
        {
            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                int length1 = Marshal.ReadInt32(bstr1, -4);
                int length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    for (int x = 0; x < length1; ++x)
                    {
                        byte b1 = Marshal.ReadByte(bstr1, x);
                        byte b2 = Marshal.ReadByte(bstr2, x);
                        if (b1 != b2) return false;
                    }
                }
                else return false;
                return true;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }
        }

        public static byte[] SecureStringToByteArray(SecureString secureString)
        {
            IntPtr unmanagedBytes = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            byte[] byteValue = null;
            try
            {
                unsafe
                {
                    byte* byteArray = (byte*)unmanagedBytes;

                    // Fetch end of the string
                    byte* pEnd = byteArray;
                    char c = '\0';
                    do
                    {
                        byte b1 = *pEnd++;
                        byte b2 = *pEnd++;
                        c = '\0';
                        c = (char)(b1 << 8);
                        c += (char)b2;
                    } while (c != '\0');

                    // Length is the difference here 
                    int length = (int)((pEnd - byteArray) - 2);
                    byteValue = new byte[length];
                    for (int i = 0; i < length; ++i)
                    {
                        byteValue[i] = *(byteArray + i);
                    }
                }
            }
            finally
            {
                // This will completely remove the data from memory
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedBytes);
            }

            return byteValue;
        }

        /// <summary>
        /// Creates a random salt that will be used to encrypt a file.
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
    }
}
