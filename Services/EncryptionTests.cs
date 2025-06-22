using System;
using System.Linq;
using AIAnywhere.Services;

namespace AIAnywhere.Tests
{
    /// <summary>
    /// Simple test class to verify portable encryption functionality
    /// </summary>
    public static class PortableEncryptionTests
    {
        public static void RunTests()
        {
            Console.WriteLine("Testing Portable Encryption Service...");

            // Test 1: Basic encryption/decryption
            string original = "sk-test-api-key-12345";
            string encrypted = PortableEncryptionService.Encrypt(original) ?? "";
            string decrypted = PortableEncryptionService.Decrypt(encrypted) ?? "";

            Console.WriteLine($"Original: {original}");
            Console.WriteLine($"Encrypted: {encrypted}");
            Console.WriteLine($"Decrypted: {decrypted}");
            Console.WriteLine(
                $"Encryption/Decryption Test: {(original == decrypted ? "PASSED" : "FAILED")}"
            );
            Console.WriteLine();

            // Test 2: Empty string handling
            string emptyEncrypted = PortableEncryptionService.Encrypt("") ?? "";
            string emptyDecrypted = PortableEncryptionService.Decrypt(emptyEncrypted) ?? "";
            Console.WriteLine($"Empty string test: {(emptyDecrypted == "" ? "PASSED" : "FAILED")}");
            Console.WriteLine();

            // Test 3: IsEncrypted detection
            bool isOriginalEncrypted = PortableEncryptionService.IsEncrypted(original);
            bool isEncryptedDetected = PortableEncryptionService.IsEncrypted(encrypted);
            Console.WriteLine(
                $"IsEncrypted detection: Original={isOriginalEncrypted}, Encrypted={isEncryptedDetected}"
            );
            Console.WriteLine(
                $"IsEncrypted test: {(!isOriginalEncrypted && isEncryptedDetected ? "PASSED" : "FAILED")}"
            );
            Console.WriteLine();

            // Test 4: Masking functionality
            string masked = PortableEncryptionService.MaskSensitiveText(original, 4);
            Console.WriteLine($"Masked key: {masked}");
            Console.WriteLine(
                $"Masking test: {(masked.StartsWith("sk-t") && masked.Contains("•") ? "PASSED" : "FAILED")}"
            );

            // Test 5: Full masking functionality
            string fullyMasked = PortableEncryptionService.MaskSensitiveTextFully(original);
            Console.WriteLine($"Fully masked key: {fullyMasked}");
            Console.WriteLine(
                $"Full masking test: {(fullyMasked.All(c => c == '•') ? "PASSED" : "FAILED")}"
            );
            Console.WriteLine("\nAll portable encryption tests completed!");
            Console.WriteLine(
                "✅ This encryption is PORTABLE - the same config.json will work on different machines!"
            );
        }
    }
}
