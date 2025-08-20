using System;
using System.Security.Cryptography;
using System.Text;

namespace VibeShootout.Backend.Services
{
    public static class ChecksumService
    {
        public static string CalculateMD5(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string CalculateDiffChecksum(string diff)
        {
            // Normalize the diff by removing timestamps and other volatile elements
            var normalizedDiff = diff
                .Replace("\r\n", "\n")  // Normalize line endings
                .Replace("\r", "\n")    // Normalize line endings
                .Trim();                // Remove leading/trailing whitespace

            return CalculateMD5(normalizedDiff);
        }
    }
}