using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace FreneticMediaServer
{
    public class SecurityHelper
    {
        public static readonly SHA512 sha = SHA512.Create();

        public static string HashV100(string input, byte[] salt)
        {
            byte[] dat = GetPbkdf2Bytes("a3597z0^^&w0fa" + input + "sa!?()sffbqnk68", salt, 100 * 1000, 32);
            return "v100:" + GeneralHelpers.BytesToHex(salt) + ":" + GeneralHelpers.BytesToHex(dat);
        }

        public static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        public static bool SlowEquals(string a, string b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        private static byte[] GetPbkdf2Bytes(string password, byte[] salt, int iterations, int outputBytes)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt);
            pbkdf2.IterationCount = iterations;
            return pbkdf2.GetBytes(outputBytes);
        }

        public static string HashCurrent(string input)
        {
            return HashV100(input, GetRandomBytes(32));
        }

        public static string HashSpecific(string type, string salt, string input)
        {
            if (type == "v100")
            {
                return HashV100(input, GeneralHelpers.HexToBytes(salt));
            }
            return null;
        }

        public static bool CheckHashValidity(string hashed_pass, string checked_pass)
        {
            string[] split = hashed_pass.Split(':');
            if (split.Length != 3)
            {
                throw new Exception("Malformed hash!");
            }
            string specific = HashSpecific(split[0], split[1], checked_pass);
            if (specific == null)
            {
                throw new Exception("Hash type invalid!");
            }
            return SlowEquals(hashed_pass, specific);
        }

        public static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        public static readonly MD5 md5 = MD5.Create();

        public static byte[] GetRandomBytes(int length = 64)
        {
            byte[] data = new byte[length];
            rng.GetBytes(data);
            return data;
        }
    }
}
