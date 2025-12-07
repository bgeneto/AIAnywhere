# AI Anywhere User Manual

Welcome to **AI Anywhere**, your universal AI assistant designed to integrate seamlessly into your workflow.

## 🚀 Getting Started

1. **Launch the App**: AI Anywhere starts minimized to the **system tray**.
2. **Open the Window**: Click the tray icon or use your configured global hotkey.
3. **Configure API**:
   * Go to **Settings > API**.
   * Enter your LLM provider's **Endpoint** (e.g., `https://api.openai.com/v1`).
   * Enter your **API Key**.
   * Click **Test Connection** to verify.
4. **Select Models (Vital)**:
   * Go to the **Models** tab in Settings.
   * Click **Refresh Models** to fetch the list from your provider.
   * **You must select a model for each category to use those features:**
     * **Text Model**: Required for chat, summarization, translation, custom tasks, etc.
     * **Image Model**: Required for "Generate Image" tasks (e.g., DALL-E 3).
     * **Audio Model (STT)**: Required for "Speech-to-Text" (transcribing audio files).
     * **TTS Model**: Required for "Read Aloud" (Text-to-Speech).

## 🏠 Home

The Home screen is your main command center.

* **Task Selection**: Choose from predefined tasks (e.g., "Email Reply", "Translate", "Generate Image") or your own **My Tasks**.
* **Task Options**: Configure task-specific options like tone, length, language, etc.
* **Prompt Input**: Type your request or review captured text.
* **Clipboard Controls**:
  * **Auto checkbox**: When checked, clipboard content automatically syncs to the prompt when the window gains focus. Uncheck to manually paste content with Ctrl+V.
  * **Sync button**: Manually refresh the prompt with current clipboard content.
  * **Clear button**: Clear the prompt and any streaming response.
* **Send**: Click **Send** (or press `Ctrl+Enter`) to process your request.
* **Token Counter**: Shows estimated token count to help avoid exceeding model limits.

## 📜 History

View and manage your past interactions.

* **Search**: Filter history by **Task Type**, **Prompt**, or **Response** content.
  * *Tip*: Search supports partial matches and ignores extra spaces.
* **Rerun**: Click the **Rerun** button on any entry to load it back into the Home screen.
* **Manage**: Expand entries to see full details, delete individual items, or **Clear All** history.

## ✨ My Tasks

Tailor AI Anywhere to your specific needs by creating custom workflows.

1. **Create**: Go to **My Tasks** and click **Create New Task**.
2. **Basic Info**:
   * **Name**: The display name in the task dropdown.
   * **Description**: A short help text explaining what the task does.
   * **System Prompt**: The instruction sent to the AI (e.g., "You are a helpful coding assistant.").
3. **Add Options**: Create dynamic form fields that appear when you use the task.
   * **Option Types**:
     * **Text**: A simple input box.
     * **Select**: A dropdown menu. You must provide comma-separated values (e.g., `Professional, Casual, Funny`).
     * **Number**: A numeric input, optionally with Min/Max limits.
   * **Key**: A unique identifier for the option (e.g., `target_lang`).
4. **Use Placeholders**:
   * To use user input in your prompt, insert the option's **Key** wrapped in curly braces into the System Prompt.
   * *Example*:
     * Option: Name="Target Language", Key=`target_lang`, Type=Select, Values=`Python, Rust, TypeScript`
     * System Prompt: `Translate the following code into {target_lang}.`

## ⚙️ Settings

Customize the app behavior.

### API & Models
* **Endpoint**: Your LLM provider's API URL.
* **API Key**: Securely stored with AES-GCM encryption.
* **Text Model**: The primary LLM for text generation.
* **Image Model**: The model used for image generation calls.
* **Audio Model**: The model used for converting speech/audio to text (e.g., `whisper-1`).
* **TTS Model**: The model used for text-to-speech.

### General
* **Global Hotkey**: Set a keyboard shortcut (e.g., `Ctrl+Space`) to trigger AI Anywhere from *any* application.
* **Paste Behavior**:
  * **Auto Paste**: Automatically pastes the AI response back into your active application.
  * **Clipboard Mode**: Copies the response to your clipboard.
  * **Review Mode**: Opens a preview window to review, edit, copy, or paste the response.
* **Disable Text Selection**: Skip automatic text capture for faster app startup.
* **Debug Logging**: Enable to diagnose API issues.

### History
* **History Limit**: Set the maximum number of entries to keep.
* **Media Retention**: Configure how long to keep generated images and audio files.

## 🖥️ System Tray

AI Anywhere runs in the system tray:

* **Left-click**: Open the main window.
* **Right-click**: Access menu (Open, Settings, About, Quit).
* **Close button (X)**: Hides to tray instead of quitting. Use **Quit** from tray menu to fully exit.

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Enter` | Send the prompt |
| `Escape` | Close review modal / Clear search |
| Global Hotkey | Open AI Anywhere from any app |

## 💡 Tips & Tricks

* **Global Access**: Use your **Global Hotkey** to invoke AI Anywhere while working in other apps. It can capture your selected text automatically!
* **Append Content**: Uncheck **Auto** to disable auto-sync, then use Ctrl+V (or Sync button) to paste and append content to the prompt.
* **Review Mode**: Use **Review Mode** for sensitive tasks where you want to check the output before it's pasted.
* **Quick Model Discovery**: Use **Get Models** button to auto-discover available models from your API.

## 🐧 Linux Wayland Notes

Due to Wayland's security model:

* **Global hotkeys**: May not work directly. Configure a system shortcut in GNOME/KDE Settings → Keyboard Shortcuts.
* **Text capture**: You must **manually press Ctrl+C** before triggering the hotkey. Wayland prevents automatic text selection capture.

**Workflow on Linux Wayland**: Select text → Ctrl+C → Open AI Anywhere → Text appears automatically.
