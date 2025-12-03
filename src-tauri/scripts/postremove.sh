#!/bin/bash
# Post-removal script for AI Anywhere
# Clean up after uninstallation

APP_NAME="ai-anywhere"

# Update desktop database
if command -v update-desktop-database &> /dev/null; then
    update-desktop-database /usr/share/applications 2>/dev/null || true
fi

# Update icon cache
if command -v gtk-update-icon-cache &> /dev/null; then
    for dir in /usr/share/icons/hicolor; do
        if [ -d "$dir" ]; then
            gtk-update-icon-cache -f -t "$dir" 2>/dev/null || true
        fi
    done
fi

# Clean up app data directory (optional - uncomment if you want to remove user data on uninstall)
# Note: This removes user configuration! Only enable if desired.
# rm -rf "$HOME/.local/share/${APP_NAME}" 2>/dev/null || true

exit 0
