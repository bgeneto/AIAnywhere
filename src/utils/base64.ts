/**
 * Utility functions for base64 encoding
 * Using chunked encoding to avoid stack overflow with large arrays
 */

/**
 * Convert Uint8Array or number array to base64 string
 * Uses chunked processing to avoid stack overflow on large arrays
 * @param bytes - The byte array to convert
 * @returns Base64 encoded string
 */
export function uint8ArrayToBase64(bytes: Uint8Array | number[]): string {
  const arr = bytes instanceof Uint8Array ? bytes : new Uint8Array(bytes);
  const CHUNK_SIZE = 8192;
  let result = '';
  for (let i = 0; i < arr.length; i += CHUNK_SIZE) {
    const chunk = arr.slice(i, i + CHUNK_SIZE);
    result += String.fromCharCode.apply(null, Array.from(chunk));
  }
  return btoa(result);
}

/**
 * Convert base64 string to Uint8Array
 * @param base64 - The base64 string to decode
 * @returns Uint8Array of bytes
 */
export function base64ToUint8Array(base64: string): Uint8Array {
  const binaryString = atob(base64);
  const bytes = new Uint8Array(binaryString.length);
  for (let i = 0; i < binaryString.length; i++) {
    bytes[i] = binaryString.charCodeAt(i);
  }
  return bytes;
}
