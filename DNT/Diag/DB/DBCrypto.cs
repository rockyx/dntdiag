using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Text;

using DNT.Diag;
namespace DNT.Diag.DB
{
	internal class DBCrypto
	{
		private static byte[] AES_CBC_KEY = {
			0xFA, 0xC2, 0xCC, 0x82, //
			0x8C, 0xFD, 0x42, 0x17, //
			0xA0, 0xB2, 0x97, 0x4D, //
			0x19, 0xC8, 0xA4, 0xB1, //
			0xF5, 0x73, 0x23, 0x7C, //
			0xB1, 0xC4, 0xC0, 0x38, //
			0xC9, 0x80, 0xB9, 0xF7, //
			0xC3, 0x3E, 0xC9, 0x12
		};

		private static byte[] AES_CBC_IV = {
			0x7C, 0xF4, 0xF0, 0x7D, //
			0x3B, 0x0D, 0xA1, 0xC6, //
			0x35, 0x74, 0x18, 0xB3, //
			0x51, 0xA3, 0x87, 0x8E
		};

		private static Dictionary<string, byte[]> encryptMap;
		private static Dictionary<string, string> decryptMap;

		private static Aes aesAlg;
		private static ICryptoTransform decryptor;
		private static ICryptoTransform encryptor;

		static DBCrypto()
		{
			encryptMap = new Dictionary<string, byte[]> ();
			decryptMap = new Dictionary<string, string> ();

			aesAlg = Aes.Create ();
			aesAlg.Key = AES_CBC_KEY;
			aesAlg.IV = AES_CBC_IV;
			aesAlg.Padding = PaddingMode.PKCS7;

			decryptor = aesAlg.CreateDecryptor (aesAlg.Key, aesAlg.IV);
			encryptor = aesAlg.CreateEncryptor (aesAlg.Key, aesAlg.IV);
		}

		internal static byte[] DecryptToBytes(byte[] cipher)
		{
			if ((cipher == null) || (cipher.Length <= 0))
				throw new ArgumentException ("Cipher Text");
			try {
				byte[] plainBytes = null;

				using (MemoryStream msDecrypt = new MemoryStream(cipher)) {
					byte[] buffer = new byte[1024];
					int readBytes = 0;
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
						using (MemoryStream utf8Memory = new MemoryStream()) {
							while ((readBytes = csDecrypt.Read(buffer, 0, buffer.Length)) > 0) {
								utf8Memory.Write(buffer, 0, readBytes);
							}
							plainBytes = utf8Memory.ToArray();
						}
					}
				}

				return plainBytes;
			} catch (Exception ex) {
				throw new CryptoException (ex.Message);
			}
		}

		internal static string DecryptToString(byte[] cipher)
		{
			byte[] plainBytes = DecryptToBytes(cipher);
			return UTF8Encoding.UTF8.GetString (plainBytes);
		}

		internal static byte[] Encrypt(byte[] plain)
		{
			if ((plain == null) || (plain.Length <= 0))
				throw new ArgumentException ("Plain Bytes");

			byte[] cipherBytes = null;

			using (MemoryStream msEncrypt = new MemoryStream ()) {
				using (CryptoStream csEncrypt = new CryptoStream (msEncrypt, encryptor, CryptoStreamMode.Write)) {
					csEncrypt.Write (plain, 0, plain.Length);
					csEncrypt.FlushFinalBlock ();
					cipherBytes = msEncrypt.ToArray ();
				}
			}

			return cipherBytes;
		}

		internal static byte[] Encrypt(string plain)
		{
			if (String.IsNullOrEmpty (plain))
				throw new ArgumentException ("Plain");

			if (!encryptMap.ContainsKey (plain)) {
				byte[] plainBytes = UTF8Encoding.UTF8.GetBytes (plain);
				encryptMap.Add (plain, Encrypt (plainBytes));
			}

			return encryptMap [plain];
		}

		internal static byte[] Language
		{
			get { return Encrypt (Settings.LanguageText); }
		}

		internal static string Decrypt(string name, string cls, byte[] cipherBytes)
		{
			string key = String.Format ("{0}_{1}_{2}", name, Settings.LanguageText, cls);

			if (!decryptMap.ContainsKey (key)) {
				decryptMap.Add (key, DecryptToString (cipherBytes));
			}

			return decryptMap [key];
		}
	}
}

