using System;
using System.Text;
using System.Collections.Generic;

namespace FreneticMediaServer
{
    public class GeneralHelpers
    {
        public static readonly UTF8Encoding EncodingUTF8 = new UTF8Encoding(false);

        public static IEnumerable<KeyValuePair<string, string>> ReadConfigData(string config, Action<string> error)
        {
            string[] file_lines = config.Replace('\r', '\n').Split('\n');
            foreach (string line_raw in file_lines)
            {
                string line = line_raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                if (line.StartsWith('#'))
                {
                    continue;
                }
                int index_equals = line.IndexOf('=');
                if (index_equals < 1)
                {
                    error(line);
                    continue;
                }
                string setting = line.Substring(0, index_equals).Trim().ToLowerInvariant();
                string value = line.Substring(index_equals + 1).Trim();
                yield return new KeyValuePair<string, string>(setting, value);
            }
        }

        static int GetHexVal(char chr)
        {
            return chr - (chr < 58 ? 48 : (chr < 97 ? 55 : 87));
        }
        
        static char GetHexChar(int val)
        {
            return (char)((val < 10) ? ('0' + val) : ('A' + (val - 10)));
        }

        public static byte[] HexToBytes(string hex)
        {
            int l = hex.Length >> 1;
            byte[] arr = new byte[l];
            for (int i = 0; i < l; i++)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1])) + (GetHexVal(hex[(i << 1) + 1]) << 4));
            }
            return arr;
        }

        public static string BytesToHex(byte[] bytes)
        {
            char[] res = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                res[i << 1] = GetHexChar(bytes[i] & 0x0F);
                res[(i << 1) + 1] = GetHexChar((bytes[i] & 0xF0) >> 4);
            }
            return new string(res);
        }

        public static string UnescapeFromConfig(string input)
        {
            return input.Replace("\\n", "\n").Replace("\\\\", "\\");
        }

        public static string EscapeForConfig(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\r\n", "\\n").Replace("\n", "\\n");
        }

        public static TimeSpan? ParseTimeSpan(string timespanText)
        {
            long multiplier;
            if (timespanText.EndsWith("S"))
            {
                multiplier = 1;
                timespanText = timespanText.Substring(0, timespanText.Length - 1);
            }
            else if (timespanText.EndsWith("M"))
            {
                multiplier = 60;
                timespanText = timespanText.Substring(0, timespanText.Length - 1);
            }
            else if (timespanText.EndsWith("H"))
            {
                multiplier = 60 * 60;
                timespanText = timespanText.Substring(0, timespanText.Length - 1);
            }
            else if (timespanText.EndsWith("D"))
            {
                multiplier = 60 * 60 * 24;
                timespanText = timespanText.Substring(0, timespanText.Length - 1);
            }
            else
            {
                return null;
            }
            if (long.TryParse(timespanText, out long result))
            {
                return new TimeSpan(result * multiplier * TimeSpan.TicksPerSecond);
            }
            return null;
        }

        public static long? ParseFileSizeLimit(string limitText)
        {
            long multiplier;
            if (limitText.EndsWith("KB"))
            {
                multiplier = 1000;
                limitText = limitText.Substring(0, limitText.Length - 2);
            }
            else if (limitText.EndsWith("MB"))
            {
                multiplier = 1000_000;
                limitText = limitText.Substring(0, limitText.Length - 2);
            }
            else if (limitText.EndsWith("GB"))
            {
                multiplier = 1000_000_000;
                limitText = limitText.Substring(0, limitText.Length - 2);
            }
            else if (limitText.EndsWith("B"))
            {
                multiplier = 1;
                limitText = limitText.Substring(0, limitText.Length - 1);
            }
            else
            {
                return null;
            }
            if (long.TryParse(limitText, out long result))
            {
                return result * multiplier;
            }
            return null;
        }
    }
}
