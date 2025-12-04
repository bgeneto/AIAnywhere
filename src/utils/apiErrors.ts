/**
 * API Error Parsing Utilities
 * Provides user-friendly, translated error messages for common API errors.
 */

import type { Translations } from '../i18n/translations';

export interface ParsedError {
  title: string;
  message: string;
}

/**
 * Parses API error messages and returns translated user-friendly error info.
 * Detects HTTP status codes and common error patterns.
 * 
 * @param error - The error string from the API response
 * @param t - The translations object
 * @returns Object with title and message for display in toast
 */
export function parseApiError(error: string | undefined | null, t: Translations): ParsedError {
  if (!error) {
    return { title: t.toast.error, message: 'Request failed' };
  }

  // Check for HTTP status codes in the error message
  // Matches patterns like: "code":401, "code": 401, status: 401, status 401
  const statusCodeMatch = error.match(/"code"\s*:\s*(\d{3})|status[:\s]+(\d{3})/i);
  const statusCode = statusCodeMatch ? parseInt(statusCodeMatch[1] || statusCodeMatch[2], 10) : null;

  // Also check for common error patterns in the message
  const lowerError = error.toLowerCase();

  // 401 Unauthorized - Authentication failed
  if (statusCode === 401 || 
      lowerError.includes('unauthorized') || 
      lowerError.includes('invalid api key') ||
      lowerError.includes('invalid_api_key') ||
      lowerError.includes('authentication') || 
      lowerError.includes('no cookie auth credentials') ||
      lowerError.includes('incorrect api key') ||
      lowerError.includes('api key is invalid')) {
    return { title: t.toast.apiAuthError, message: t.toast.apiAuthErrorDesc };
  }

  // 403 Forbidden - Access denied
  if (statusCode === 403 || 
      lowerError.includes('forbidden') || 
      lowerError.includes('access denied') ||
      lowerError.includes('permission denied') ||
      lowerError.includes('insufficient') ||
      lowerError.includes('not allowed')) {
    return { title: t.toast.apiForbiddenError, message: t.toast.apiForbiddenErrorDesc };
  }

  // 404 Not Found - Endpoint not found
  if (statusCode === 404 || 
      lowerError.includes('not found') || 
      lowerError.includes('no such') ||
      lowerError.includes('does not exist')) {
    return { title: t.toast.apiNotFoundError, message: t.toast.apiNotFoundErrorDesc };
  }

  // 429 Rate Limit - Too many requests
  if (statusCode === 429 || 
      lowerError.includes('rate limit') || 
      lowerError.includes('rate_limit') ||
      lowerError.includes('too many requests') ||
      lowerError.includes('quota exceeded') ||
      lowerError.includes('requests_limit')) {
    return { title: t.toast.apiRateLimitError, message: t.toast.apiRateLimitErrorDesc };
  }

  // 5xx Server errors
  if ((statusCode && statusCode >= 500 && statusCode < 600) || 
      lowerError.includes('server error') ||
      lowerError.includes('internal error') ||
      lowerError.includes('internal_error') ||
      lowerError.includes('service unavailable') ||
      lowerError.includes('bad gateway')) {
    return { title: t.toast.apiServerError, message: t.toast.apiServerErrorDesc };
  }

  // Network/Connection errors
  if (lowerError.includes('network') || 
      lowerError.includes('connection refused') ||
      lowerError.includes('connection reset') ||
      lowerError.includes('failed to fetch') ||
      lowerError.includes('unable to connect') || 
      lowerError.includes('dns') ||
      lowerError.includes('econnrefused') ||
      lowerError.includes('enotfound')) {
    return { title: t.toast.apiNetworkError, message: t.toast.apiNetworkErrorDesc };
  }

  // Timeout errors
  if (lowerError.includes('timeout') || 
      lowerError.includes('timed out') ||
      lowerError.includes('deadline exceeded') ||
      lowerError.includes('etimedout')) {
    return { title: t.toast.apiTimeoutError, message: t.toast.apiTimeoutErrorDesc };
  }

  // Default: return the original error message
  return { title: t.toast.error, message: error };
}
