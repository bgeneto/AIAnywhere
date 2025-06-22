using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AIAnywhere.Services
{
    /// <summary>
    /// Provides encryption and decryption services for sensitive data like API keys.
    /// Uses AES encryption with a machine-specific key for security.
    /// </summary>
    public static class EncryptionService
    {
        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes(
            "AIAnywhere_SecureKey_v1.0"
        );

        /// <summary>
        /// Encrypts a plain text string using Windows Data Protection API (DPAPI).
        /// This ensures the encrypted data can only be decrypted on the same machine by the same user.
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded encrypted string</returns>
        public static string? Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(
                    plainTextBytes,
                    _entropy,
                    DataProtectionScope.CurrentUser
                );
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                // If encryption fails, return original text (fallback for compatibility)
                return plainText;
            }
        }

        /// <summary>
        /// Decrypts an encrypted string using Windows Data Protection API (DPAPI).
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted string</param>
        /// <returns>Decrypted plain text string</returns>
        public static string? Decrypt(string? encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    _entropy,
                    DataProtectionScope.CurrentUser
                );
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                // If decryption fails, assume it's already plain text (for backward compatibility)
                return encryptedText;
            }
        }

        /// <summary>
        /// Checks if a string appears to be encrypted (Base64 format).
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if the text appears to be encrypted</returns>
        public static bool IsEncrypted(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            try
            {
                // Try to parse as Base64 - if successful and length is reasonable, likely encrypted
                byte[] data = Convert.FromBase64String(text);
                return data.Length > 0 && text.Length > 20; // Encrypted strings are typically longer
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
        public static string MaskSensitiveText(string sensitiveText, int visibleChars = 4)
        {
            if (string.IsNullOrEmpty(sensitiveText))
                return string.Empty;

            if (sensitiveText.Length <= visibleChars)
                return new string('•', sensitiveText.Length);

            return sensitiveText.Substring(0, visibleChars)
                + new string('•', Math.Max(8, sensitiveText.Length - visibleChars));
        }
    }
}
