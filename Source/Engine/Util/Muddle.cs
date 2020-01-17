using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine.Util
{
    /// <summary>
    /// Methods to encrypt and decrypt data with keys.
    /// </summary>
    public class Muddle
    {
        static byte[] key1, key2;

        /// <summary>
        /// Assign encryption keys in format "aabbccdd-eeffgghh-iijjkkll-mmnnoopp".
        /// </summary>
        public static void SetKeys(string key1String, string key2String)
        {
            string ks;
            ks = key1String.Replace("-", "");
            key1 = Enumerable.Range(0, ks.Length / 2).Select(x => Convert.ToByte(ks.Substring(x * 2, 2), 16)).ToArray();
            ks = key2String.Replace("-", "");
            key2 = Enumerable.Range(0, ks.Length / 2).Select(x => Convert.ToByte(ks.Substring(x * 2, 2), 16)).ToArray();
        }

        /// <summary>
        /// Randomly generate new encryption keys.
        /// </summary>
        public static void NewKeys()
        {
            Random rand = new Random();
            key1 = new byte[16];
            key2 = new byte[16];
            for (int i = 0; i < key1.Length; i++)
            {
                key1[i] = (byte)rand.Next(0, 256);
                key2[i] = (byte)rand.Next(0, 256);
            }
        }

        /// <summary>
        /// Encrypt source file and store in destination file. Returns true if successful.
        /// </summary>
        public static bool Encrypt(string source, string destination)
        {
            if (!File.Exists(source)) return false;
            using (FileStream fs = new FileStream(source, FileMode.Open, FileAccess.Read))
            {
                if (!Encrypt(fs, out Stream encrypted)) return false;
                using (FileStream fs2 = new FileStream(destination, FileMode.Create))
                {
                    encrypted.Position = 0;
                    encrypted.CopyTo(fs2);
                    return true;
                }
            }
        }

        /// <summary>
        /// Encrypt input stream and store in result stream. Returns true if successful.
        /// </summary>
        public static bool Encrypt(Stream input, out Stream result)
        {
            input.Position = 0;
            result = new MemoryStream();
            byte[] buffer = new byte[16];
            //byte[] key1 = new byte[16];
            //byte[] key2 = new byte[16];
            int key1Pos = 0;
            int key2Pos = 0;

            byte[] bytes = Encoding.ASCII.GetBytes("XXZ16\0");
            result.Write(bytes, 0, bytes.Length);
            result.Write(key1, 0, key1.Length);
            result.Write(key2, 0, key2.Length);

            while (true)
            {
                int readLength = input.Read(buffer, 0, buffer.Length);
                if (readLength <= 0) break;
                for (int i = 0; i < readLength; i++)
                {
                    buffer[i] = (byte)((buffer[i] + key1[key1Pos] + key2[(key1Pos + key2Pos) % key2.Length]) % 256);
                    key1Pos++;
                    if (key1Pos >= key1.Length)
                    {
                        key1Pos = 0;
                        key2Pos++;
                        if (key2Pos >= key2.Length)
                        {
                            key2Pos = 0;
                        }
                    }
                }
                result.Write(buffer, 0, buffer.Length);
            }

            return true;
            // TODO: Check for failure?
        }

        /// <summary>
        /// Decrypt source file and store in destination file. Returns true if successful.
        /// </summary>
        public static bool Decrypt(string source, string destination)
        {
            if (!File.Exists(source)) return false;
            using (FileStream fs = new FileStream(source, FileMode.Open))
            {
                if (!Decrypt(fs, out Stream decrypted)) return false;
                using (FileStream fs2 = new FileStream(destination, FileMode.Create))
                {
                    decrypted.Position = 0;
                    decrypted.CopyTo(fs2);
                    return true;
                }
            }
        }

        /// <summary>
        /// Decrypt source file and store in result stream. Returns true if successful.
        /// </summary>
        public static bool Decrypt(string source, out Stream result)
        {
            result = new MemoryStream();
            if (!File.Exists(source)) return false;
            using (FileStream fs = new FileStream(source, FileMode.Open))
            {
                return Decrypt(fs, out result);
            }
        }

        /// <summary>
        /// Decrypt input stream and store in result stream. Returns true if successful.
        /// </summary>
        public static bool Decrypt(Stream input, out Stream result)
        {
            input.Position = 0;
            result = new MemoryStream();
            byte[] buffer = new byte[16];
            byte[] key1 = new byte[16];
            byte[] key2 = new byte[16];
            int key1Pos = 0;
            int key2Pos = 0;

            int readLength = input.Read(buffer, 0, 6);
            if (readLength != 6) return false;
            if (Encoding.ASCII.GetString(buffer, 0, 6) != "XXZ16\0") return false;
            readLength = input.Read(key1, 0, 16);
            if (readLength != 16) return false;
            readLength = input.Read(key2, 0, 16);
            if (readLength != 16) return false;

            while (true)
            {
                readLength = input.Read(buffer, 0, buffer.Length);
                if (readLength <= 0) break;
                for (int i = 0; i < readLength; i++)
                {
                    buffer[i] = (byte)((buffer[i] + 512 - key1[key1Pos] - key2[(key1Pos + key2Pos) % key2.Length]) % 256);
                    key1Pos++;
                    if (key1Pos >= key1.Length)
                    {
                        key1Pos = 0;
                        key2Pos++;
                        if (key2Pos >= key2.Length)
                        {
                            key2Pos = 0;
                        }
                    }
                }
                result.Write(buffer, 0, buffer.Length);
            }

            return true;
        }
    }
}
