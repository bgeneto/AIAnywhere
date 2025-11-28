using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AIAnywhere.Services
{
    /// <summary>
    /// Provides portable encryption and decryption services for sensitive data like API keys.
    /// Uses AES encryption that works across different machines for portable applications.
    /// </summary>
    public static class PortableEncryptionService
    {
        // Base entropy for key derivation - can be changed to invalidate all existing encrypted data
        private static readonly string _baseEntropy = "AIAnywhere_Portable_v1.0";

        /// <summary>
        /// Generates a deterministic key for encryption/decryption
        /// This provides reasonable security while maintaining portability
        /// </summary>
        private static byte[] GetEncryptionKey()
        {
            // Create a deterministic key that's reasonably secure but portable
            // Uses the application identifier + a fixed secret
            var keySource = _baseEntropy + "SecurePortableKey2025";

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(keySource));
            }
        }

        /// <summary>
        /// Encrypts a plain text string using AES encryption.
        /// The result is portable across different machines.
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded encrypted string with IV prepended</returns>
        public static string? Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = GetEncryptionKey();
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Prepend IV to the encrypted data
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (
                            var csEncrypt = new CryptoStream(
                                msEncrypt,
                                encryptor,
                                CryptoStreamMode.Write
                            )
                        )
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch
            {
                // If encryption fails, return original text (fallback for compatibility)
                return plainText;
            }
        }

        /// <summary>
        /// Decrypts an AES encrypted string.
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted string with IV prepended</param>
        /// <returns>Decrypted plain text string</returns>
        public static string? Decrypt(string? encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                var fullCipherBytes = Convert.FromBase64String(encryptedText);

                using (var aes = Aes.Create())
                {
                    aes.Key = GetEncryptionKey();

                    // Extract IV from the beginning of the encrypted data
                    var iv = new byte[aes.IV.Length];
                    var cipherBytes = new byte[fullCipherBytes.Length - iv.Length];

                    Array.Copy(fullCipherBytes, 0, iv, 0, iv.Length);
                    Array.Copy(fullCipherBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(cipherBytes))
                    using (
                        var csDecrypt = new CryptoStream(
                            msDecrypt,
                            decryptor,
                            CryptoStreamMode.Read
                        )
                    )
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch
            {
                // If decryption fails, assume it's already plain text (for backward compatibility)
                return encryptedText;
            }
        }

        /// <summary>
        /// Checks if a string appears to be encrypted (Base64 format with reasonable length).
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if the text appears to be encrypted</returns>
        public static bool IsEncrypted(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            try
            {
                // Try to parse as Base64 - if successful and length is reasonable, likely encrypted
                byte[] data = Convert.FromBase64String(text);
                // AES encrypted data with IV should be at least 32 bytes (16 IV + 16+ encrypted)
                return data.Length >= 32 && text.Length > 40;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a masked version of sensitive text for display purposes.
        /// </summary>
        /// <param name="sensitiveText">The sensitive text to mask</param>
        /// <param name="visibleChars">Number of characters to show at the beginning</param>
        /// <returns>Masked string with dots</returns>
        public static string MaskSensitiveText(string? sensitiveText, int visibleChars = 4)
        {
            if (string.IsNullOrEmpty(sensitiveText))
                return string.Empty;

            if (sensitiveText.Length <= visibleChars)
                return new string('•', Math.Max(8, sensitiveText.Length));

            return sensitiveText.Substring(0, visibleChars)
                + new string('•', Math.Max(8, sensitiveText.Length - visibleChars));
        }

        /// <summary>
        /// Creates a fully masked version of sensitive text (all dots).
        /// </summary>
        /// <param name="sensitiveText">The sensitive text to mask</param>
        /// <returns>Masked string with all dots</returns>
        public static string MaskSensitiveTextFully(string? sensitiveText)
        {
            if (string.IsNullOrEmpty(sensitiveText))
                return string.Empty;

            return new string('•', Math.Max(12, Math.Min(24, sensitiveText.Length)));
        }
    }
}
