//! Encryption module for AI Anywhere
//! Provides portable AES encryption for sensitive data like API keys

use aes_gcm::{
    aead::{Aead, KeyInit},
    Aes256Gcm, Nonce,
};
use base64::{engine::general_purpose::STANDARD, Engine};
use rand::Rng;
use sha2::{Digest, Sha256};

/// Base entropy for key derivation - matches .NET implementation
const BASE_ENTROPY: &str = "AIAnywhere_Portable_v1.0";
const SECRET_KEY: &str = "SecurePortableKey2025";

/// Generate a deterministic encryption key using SHA-256
/// This provides reasonable security while maintaining portability across machines
fn get_encryption_key() -> [u8; 32] {
    let key_source = format!("{}{}", BASE_ENTROPY, SECRET_KEY);
    let mut hasher = Sha256::new();
    hasher.update(key_source.as_bytes());
    let result = hasher.finalize();
    
    let mut key = [0u8; 32];
    key.copy_from_slice(&result);
    key
}

/// Encrypt a plain text string using AES-256-GCM
/// Returns Base64 encoded encrypted string with nonce prepended
pub fn encrypt(plain_text: &str) -> Result<String, String> {
    if plain_text.is_empty() {
        return Ok(String::new());
    }
    
    let key = get_encryption_key();
    let cipher = Aes256Gcm::new_from_slice(&key)
        .map_err(|e| format!("Failed to create cipher: {}", e))?;
    
    // Generate random 12-byte nonce (96 bits for GCM)
    let mut nonce_bytes = [0u8; 12];
    rand::thread_rng().fill(&mut nonce_bytes);
    let nonce = Nonce::from_slice(&nonce_bytes);
    
    // Encrypt the data
    let ciphertext = cipher
        .encrypt(nonce, plain_text.as_bytes())
        .map_err(|e| format!("Encryption failed: {}", e))?;
    
    // Prepend nonce to ciphertext
    let mut result = Vec::with_capacity(nonce_bytes.len() + ciphertext.len());
    result.extend_from_slice(&nonce_bytes);
    result.extend_from_slice(&ciphertext);
    
    // Return Base64 encoded
    Ok(STANDARD.encode(&result))
}

/// Decrypt an AES-256-GCM encrypted string
/// Expects Base64 encoded string with nonce prepended
pub fn decrypt(encrypted_text: &str) -> Result<String, String> {
    if encrypted_text.is_empty() {
        return Ok(String::new());
    }
    
    // Try to decode as Base64
    let full_cipher_bytes = STANDARD
        .decode(encrypted_text)
        .map_err(|_| {
            // If Base64 decode fails, might be plain text (migration case)
            "Not a valid encrypted string".to_string()
        })?;
    
    // Minimum length: 12 bytes nonce + at least 1 byte ciphertext + 16 bytes auth tag
    if full_cipher_bytes.len() < 29 {
        // Likely plain text, return as-is for migration compatibility
        return Ok(encrypted_text.to_string());
    }
    
    let key = get_encryption_key();
    let cipher = Aes256Gcm::new_from_slice(&key)
        .map_err(|e| format!("Failed to create cipher: {}", e))?;
    
    // Extract nonce (first 12 bytes)
    let nonce = Nonce::from_slice(&full_cipher_bytes[..12]);
    let ciphertext = &full_cipher_bytes[12..];
    
    // Decrypt
    let plaintext = cipher
        .decrypt(nonce, ciphertext)
        .map_err(|_| {
            // Decryption failed - might be plain text for migration
            "Decryption failed - may be unencrypted".to_string()
        })?;
    
    String::from_utf8(plaintext)
        .map_err(|e| format!("Invalid UTF-8 in decrypted data: {}", e))
}

/// Mask sensitive text for display (e.g., API keys)
pub fn mask_text(text: &str, visible_chars: usize) -> String {
    if text.is_empty() {
        return String::new();
    }
    
    if text.len() <= visible_chars * 2 {
        return "*".repeat(text.len());
    }
    
    let start: String = text.chars().take(visible_chars).collect();
    let end: String = text.chars().skip(text.len() - visible_chars).collect();
    let middle_len = text.len() - (visible_chars * 2);
    
    format!("{}{}{}", start, "*".repeat(middle_len), end)
}

#[cfg(test)]
mod tests {
    use super::*;
    
    #[test]
    fn test_encrypt_decrypt_roundtrip() {
        let original = "sk-test-api-key-12345";
        let encrypted = encrypt(original).unwrap();
        let decrypted = decrypt(&encrypted).unwrap();
        assert_eq!(original, decrypted);
    }
    
    #[test]
    fn test_empty_string() {
        let encrypted = encrypt("").unwrap();
        assert_eq!(encrypted, "");
        let decrypted = decrypt("").unwrap();
        assert_eq!(decrypted, "");
    }
    
    #[test]
    fn test_mask_text() {
        let masked = mask_text("sk-1234567890abcdef", 4);
        assert!(masked.starts_with("sk-1"));
        assert!(masked.ends_with("cdef"));
        assert!(masked.contains("*"));
    }
}
