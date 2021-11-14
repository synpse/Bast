# BAST

![]()
Bast is a cross-platform security tool used to bulk encrypt and decrypt files.

#### :heavy_check_mark: Features
- **Enterprise grade encryption.** Bast uses AES256 encryption with a CBC cypher mode;
- **Custom key generation.** User defined passwords are repeatedly hashed with a salt and with high iteration counts before being turned into a key;
- **Security. Always!** When encrypting, old files are overriden with junk data before being deleted. Old files become unrecoverable.
- **Multi-platform.** Built to run on both Windows and Linux based platforms (not tested on MacOS)
- **Worry-Free.** Sometimes the worst happens. Your drive fails, light goes out. Bast has you covered, each file is only safely deleted after each encrytion or decryption is successful so that no loss of data ever occurs.
- **Easy to use.** Bast is ran with one single command and requires no setup;
- **Blazing fast!** Every file is encrypted and decrypted on its own thread, the only limit is your storage drive´s speed;
- **Free and Open-Source.** Now and always!

#### :small_blue_diamond: Usage
- Clone Bast´s repo and compile the code yourself
- On your terminal of choice call Bast. (BAST_EXECUTABLE E/D PATH_TO_FILE_OR_FOLDER) 
  E => Encrypt
  D => Decrypt
  Example: ./PATH_TO_BAST/Bast.exe E ./PATH_TO_FILE_OR_FOLDER

#### :heavy_exclamation_mark: Warnings

This piece of software is currently under active development. Expect bugs, issues and everything else that is normal on unfinished software.

Use Bast at your own risk. I´m not responsible for the loss of files or data corruption. No warranties are given.

You are responsible for remembering the passwords you use. Once you encrypt a file, if you forget your password, I CANNOT recover the file for you. It is lost forever. Ever.

I´m not responsible for any kind of ramsomware attacks made using Bast or software derived from this tool. I highly condemn such practices.

