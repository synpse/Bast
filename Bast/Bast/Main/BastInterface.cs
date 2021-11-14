using Bast.Common.Enums;
using Bast.Common.Utils;
using Bast.Operations;
using Bast.Operations.Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace Bast.Main
{
    public class BastInterface
    {
        private readonly List<string> args;
        private readonly IOperationManager encryptionManager;

        public BastInterface(List<string> args)
        {
            this.args = args;
            this.encryptionManager = new AESCBCEncryptionManager();
        }

        public void Init()
        {
            Console.Clear();

            if (args.Count != 2)
            {
                Console.WriteLine(
                    "Usage:\n" +
                    "Argument 1 => E - Encrypt or D - Decrypt.\n" +
                    "Argument 2 => Path to file or folder.\n" +
                    "Example: Bast.exe E \"C:\\MyFolder\\MyFile.txt\"");
            }
            else
            {
                switch (args[0].ToUpperInvariant())
                {
                    case nameof(OperationKindAbrev.E):
                        FileOrFolderEncrypt(args);
                        break;

                    case nameof(OperationKindAbrev.D):
                        FileOrFolderDecrypt(args);
                        break;

                    default:
                        Console.WriteLine("Usage:\nArgument 1 => E - Encrypt or D - Decrypt.");
                        break;
                }
            }

            Console.WriteLine(ANSIColors.Blue("Done. Shutting down..."));
        }

        private void FileOrFolderEncrypt(List<string> args)
        {
            Console.Clear();

            List<string> file;
            FileAttributes fas;

            try
            {
                fas = File.GetAttributes(args[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ANSIColors.Red("File or folder not found or path is invalid.") + ex.HResult);
                return;
            }

            if ((fas & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Console.WriteLine(ANSIColors.DarkYellow("Folder mode enabled. Files included:"));
                file = DirectoryUtils.DirRoutine(args[1]);

                Console.WriteLine($"{file.Count} files found. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                Console.Write("Set a password: ");
                SecureString password = SecurityUtils.AskPassword();

                Console.Write("\nConfirm password: ");
                SecureString confirmPw = SecurityUtils.AskPassword();

                Console.Clear();

                if (SecurityUtils.PasswordsMatch(password, confirmPw))
                {
                    confirmPw.Clear();
                    encryptionManager.LaunchOperation(OperationKind.Encrypt, file, password);
                }
                else
                {
                    Console.WriteLine(ANSIColors.Red("Passwords do not match! Returning..."));
                }
            }
            else
            {
                file = new List<string>();
                file.Add(args[1]);

                Console.Write("Set a password: ");
                SecureString password = SecurityUtils.AskPassword();

                Console.Write("\nConfirm password: ");
                SecureString confirmPw = SecurityUtils.AskPassword();

                Console.Clear();

                if (SecurityUtils.PasswordsMatch(password, confirmPw))
                {
                    confirmPw.Clear();
                    encryptionManager.LaunchOperation(OperationKind.Encrypt, file, password);
                }
                else
                {
                    Console.WriteLine(ANSIColors.Red("Passwords do not match! Returning..."));
                }
            }
        }

        private void FileOrFolderDecrypt(List<string> args)
        {
            Console.Clear();

            List<string> file;
            FileAttributes fas;

            try
            {
                fas = File.GetAttributes(args[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ANSIColors.Red("File or folder not found or path is invalid.") + ex.HResult);
                return;
            }

            if ((fas & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Console.WriteLine(ANSIColors.DarkYellow("Folder mode enabled. Files included:"));
                file = DirectoryUtils.DirRoutine(args[1]);

                Console.WriteLine($"{file.Count} files found. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                Console.Write("Password: ");
                SecureString password = SecurityUtils.AskPassword();

                Console.Clear();

                encryptionManager.LaunchOperation(OperationKind.Decrypt, file, password);
            }
            else
            {
                file = new List<string>();
                file.Add(args[1]);

                Console.Write("Password: ");
                SecureString password = SecurityUtils.AskPassword();

                Console.Clear();

                encryptionManager.LaunchOperation(OperationKind.Decrypt, file, password);
            }
        }
    }
}
