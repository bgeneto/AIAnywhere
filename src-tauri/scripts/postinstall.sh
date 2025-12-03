#!/bin/bash
# Post-installation script for AI Anywhere
# This script fixes SELinux labels on Fedora and other SELinux-enabled distributions

# Application name (should match productName in tauri.conf.json, converted to lowercase with hyphens)
APP_NAME="ai-anywhere"
APP_BINARY="/usr/bin/${APP_NAME}"

# Check if SELinux is available and enabled
if command -v getenforce &> /dev/null; then
    SELINUX_STATUS=$(getenforce 2>/dev/null || echo "Disabled")
    
    if [ "$SELINUX_STATUS" != "Disabled" ]; then
        echo "SELinux is ${SELINUX_STATUS}. Applying SELinux context fixes..."
        
        # Fix SELinux context for the main binary
        if [ -f "$APP_BINARY" ]; then
            # restorecon restores the default SELinux security context for files
            if command -v restorecon &> /dev/null; then
                restorecon -v "$APP_BINARY" 2>/dev/null || true
                echo "Applied SELinux context to ${APP_BINARY}"
            fi
            
            # Alternative: Set the bin_t context explicitly if restorecon doesn't work
            # This allows the binary to be executed properly
            if command -v chcon &> /dev/null; then
                chcon -t bin_t "$APP_BINARY" 2>/dev/null || true
            fi
        fi
        
        # Fix SELinux context for any libraries in /usr/lib
        APP_LIB_DIR="/usr/lib/${APP_NAME}"
        if [ -d "$APP_LIB_DIR" ]; then
            if command -v restorecon &> /dev/null; then
                restorecon -Rv "$APP_LIB_DIR" 2>/dev/null || true
                echo "Applied SELinux context to ${APP_LIB_DIR}"
            fi
        fi
        
        # Also check /opt location (some packages install there)
        APP_OPT_DIR="/opt/${APP_NAME}"
        if [ -d "$APP_OPT_DIR" ]; then
            if command -v restorecon &> /dev/null; then
                restorecon -Rv "$APP_OPT_DIR" 2>/dev/null || true
                echo "Applied SELinux context to ${APP_OPT_DIR}"
            fi
        fi
        
        echo "SELinux context fixes completed."
    else
        echo "SELinux is disabled. Skipping SELinux context fixes."
    fi
else
    echo "SELinux tools not found. Skipping SELinux context fixes."
fi

# Update desktop database (for .desktop file integration)
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

exit 0
