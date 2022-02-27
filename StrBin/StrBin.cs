using System;
using System.IO;
using System.Linq;

namespace StrBin
{
    /// <summary>
    /// 文字列バイナリ変換
    /// </summary>
    internal static class StrBin
    {
        /// <summary>
        /// バイナリファイルから16進数テキストファイルに変換
        /// </summary>
        /// <param name="input">入力ファイルパス</param>
        /// <param name="prefix">16進数文字列のプレフィクス</param>
        /// <param name="separator">16進数文字列の区切り文字</param>
        /// <param name="output">出力ファイルパス</param>
        /// <exception cref="StrBinException">例外</exception>
        public static void BinaryFileToTextFile(string input, string prefix, string separator, string output)
        {
            var str = StrBin.BinaryFileToHexString(input, prefix, separator);

            var dir = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                throw new StrBinException($"Directory({dir}) is nothing.");
            }

            using (var stream = new FileStream(output, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(str);
            }
        }

        /// <summary>
        /// バイナリファイルから16進数文字列に変換
        /// </summary>
        /// <param name="input">入力ファイルパス</param>
        /// <param name="prefix">16進数文字列のプレフィクス</param>
        /// <param name="separator">16進数文字列の区切り文字</param>
        /// <returns>16進数文字列</returns>
        /// <exception cref="StrBinException">例外</exception>
        public static string BinaryFileToHexString(string input, string prefix, string separator)
        {
            if (!File.Exists(input))
            {
                throw new StrBinException($"File({input}) is nothing.");
            }

            using (var stream = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buf = new byte[stream.Length];
                stream.Read(buf, 0, buf.Length);
                return StrBin.ToString(buf, prefix, separator);
            }
        }

        /// <summary>
        /// 16進数テキストファイルからバイナリファイルに変換
        /// </summary>
        /// <param name="input">入力ファイルパス</param>
        /// <param name="output">出力ファイルパス</param>
        /// <exception cref="StrBinException">例外</exception>
        public static void TextFileToBinaryFile(string input, string output)
        {
            if (!File.Exists(input))
            {
                throw new StrBinException($"File({input}) is nothing.");
            }

            using (var stream = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream))
            {
                StrBin.HexStringBinaryFile(reader.ReadToEnd(), output);
            }
        }

        /// <summary>
        /// 16進数文字列からバイナリファイルに変換
        /// </summary>
        /// <param name="str">16進数文字列</param>
        /// <param name="output">出力ファイルパス</param>
        /// <exception cref="StrBinException">例外</exception>
        public static void HexStringBinaryFile(string str, string output)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new StrBinException($"Input string is empty.");
            }

            var dir = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                throw new StrBinException($"Directory({dir}) is nothing.");
            }

            using (var stream = new FileStream(output, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var data = StrBin.ToBinary(str);
                stream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// 16進数文字列からバイト配列に変換
        /// </summary>
        /// <param name="hex">16進数文字列</param>
        /// <returns>バイト配列</returns>
        /// <exception cref="StrBinException">例外</exception>
        public static byte[] ToBinary(string hex)
        {
            var len = (hex.Length + 1) / 2;
            var str = hex.PadLeft(len * 2, '0');
            try
            {
                return Enumerable.Range(0, len)
                                 .Select(n => Convert.ToByte(str.Substring(n * 2, 2), 16))
                                 .ToArray();
            } 
            catch (Exception)
            {
                throw new StrBinException("Input string is invalid.");
            }
        }

        /// <summary>
        /// バイト配列から16進数文字列に変換
        /// </summary>
        /// <param name="bin">バイト配列</param>
        /// <param name="prefix">16進数文字列のプレフィクス</param>
        /// <param name="separator">16進数文字列の区切り文字</param>
        /// <returns>16進数文字列</returns>
        public static string ToString(byte[] bin, string prefix, string separator)
        {
            return string.Join(separator, bin.Select(b => $"{prefix}{b:X2}"));
        }
    }

    /// <summary>
    /// StrBin例外
    /// </summary>
    internal class StrBinException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">メッセージ</param>
        public StrBinException(string message) : base(message) { }
    }
}
