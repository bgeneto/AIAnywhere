# AI Anywhere User Manual

Welcome to **AI Anywhere**, your universal AI assistant designed to integrate seamlessly into your workflow.

## 🚀 Getting Started

1.  **Launch the App**: Open AI Anywhere.
2.  **Configure API**: Go to **Settings > API** and enter your LLM provider's **Endpoint** and **API Key**. Click **Test Connection** to verify.
3.  **Select Models**: Go to **Settings > Models** and select your preferred models for Text, Image, and Audio.

## 🏠 Home

The Home screen is your main command center.

*   **Task Selection**: Choose from predefined tasks (e.g., "Email Reply", "Translate") or your own **Custom Tasks**.
*   **Prompt Input**: Type your request in the text box.
*   **Sync Clipboard**: Click **Sync** to instantly paste your current clipboard content into the prompt.
*   **Send**: Click **Send** (or press `Ctrl+Enter`) to process your request.

## 📜 History

View and manage your past interactions.

*   **Search**: Filter history by **Task Type** (e.g., "Code transpile"), **Prompt**, or **Response** content.
    *   *Tip*: You can search for "custom" to find your custom tasks.
*   **Rerun**: Click the **Rerun** button on any entry to load it back into the Home screen.
*   **Manage**: Expand entries to see full details, delete individual items, or **Clear All** history.

## ✨ Custom Tasks

Tailor AI Anywhere to your specific needs.

*   **Create**: Go to **My Tasks** and click **Create New Task**.
*   **Configure**: Give it a name, description, and a **System Prompt** that defines its behavior.
*   **Options**: Add custom variables (e.g., "Target Language", "Tone") that will appear as form fields when you select the task.

## ⚙️ Settings

Customize the app behavior.

### General
*   **Global Hotkey**: Set a keyboard shortcut to trigger AI Anywhere from *any* application.
*   **Paste Behavior**:
    *   **Auto Paste**: Automatically pastes the AI response back into your active application.
    *   **Clipboard Mode**: Copies the response to your clipboard.
    *   **Review Mode**: Opens a window to review the response before copying/pasting.
*   **Disable Text Selection**: Check this if you don't want the app to automatically capture selected text when triggered via hotkey.

### History
*   **History Limit**: Set the maximum number of entries to keep.
*   **Media Retention**: Configure how long to keep generated images and audio files.

## 💡 Tips & Tricks

*   **Global Access**: Use your **Global Hotkey** to invoke AI Anywhere while working in other apps. It can capture your selected text automatically!
*   **Search Smart**: The history search supports partial matches and ignores extra spaces (e.g., "code   fix" finds "Code Fix").
*   **Review Mode**: Use **Review Mode** for sensitive tasks where you want to check the output before it's pasted.
