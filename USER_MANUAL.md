# AI Anywhere User Manual

Welcome to **AI Anywhere**, your universal AI assistant designed to integrate seamlessly into your workflow.

## 🚀 Getting Started

1. **Launch the App**: AI Anywhere starts minimized to the **system tray**.
2. **Open the Window**: Click the tray icon or use your configured global hotkey.
3. **Configure API**: Go to **Settings > API** and enter your LLM provider's **Endpoint** and **API Key**. Click **Test Connection** to verify.
4. **Select Models**: Click **Get Models** to discover available models, then select your preferred models for Text, Image, and Audio.

## 🏠 Home

The Home screen is your main command center.

* **Task Selection**: Choose from predefined tasks (e.g., "Email Reply", "Translate") or your own **My Tasks**.
* **Task Options**: Configure task-specific options like tone, length, language, etc.
* **Prompt Input**: Type your request in the text box.
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

Tailor AI Anywhere to your specific needs.

* **Create**: Go to **My Tasks** and click **Create New Task**.
* **Configure**: Give it a name, description, and a **System Prompt** that defines its behavior.
* **Options**: Add custom variables (e.g., "Target Language", "Tone") that will appear as form fields when you select the task. Use `{placeholder}` syntax in the system prompt.
* **Import/Export**: Export your tasks to JSON for backup or import tasks from others.

## ⚙️ Settings

Customize the app behavior.

### API
* **Endpoint**: Your LLM provider's API URL (e.g., `https://api.openai.com/v1`).
* **API Key**: Securely stored with AES-GCM encryption.
* **Models**: Select models for Text, Image (DALL-E), Audio (Whisper), and Text-to-Speech.

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

**Workflow on Wayland**: Select text → Ctrl+C → Open AI Anywhere → Text appears automatically.
