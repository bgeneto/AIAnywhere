# AI Anywhere - Debug API Responses

The AI Anywhere application now includes comprehensive API response debugging to help identify JSON parsing errors and API compatibility issues.

## How to Enable Debug Logging

The debug logging is automatically enabled in your LLMService. When you use the "Custom Task" operation or any other AI feature, debug logs will be created in the `api_debug` folder next to your AIAnywhere.exe file.

## What Gets Logged

1. **Request Details**: Operation type, model, prompts, and parameters
2. **Response Content**: Full API responses with structure analysis
3. **JSON Validation**: Whether the response is valid JSON and its structure
4. **Error Details**: Complete exception information including stack traces
5. **Configuration**: Your API base URL and model settings

## Debug Log Location

Debug logs are saved to: `[AIAnywhere directory]/api_debug/api_debug_[timestamp].log`

## Testing the Debug System

1. **Run AIAnywhere.exe**
2. **Try a Custom Task operation** that causes the JSON error
3. **Check the api_debug folder** for detailed logs
4. **Send the logs** for analysis to identify the exact cause

## Log File Format

Each log contains:
- Timestamp and endpoint information
- Your configuration (API URL, models)
- Request details
- Full response content
- JSON structure analysis
- Any exceptions with stack traces

## What to Look For

The logs will help identify:
- **Invalid JSON responses** from custom API endpoints
- **Missing required fields** (like numeric fields returning null)
- **Authentication issues** causing error responses
- **Endpoint compatibility** problems with different AI providers

## Next Steps

1. Reproduce the JSON parsing error
2. Locate the corresponding debug log file
3. Share the log content to identify the root cause
4. Apply targeted fixes based on the findings

The debug system captures everything needed to diagnose API compatibility issues without compromising your API keys or sensitive data.
