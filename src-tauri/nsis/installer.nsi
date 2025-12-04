; AI Anywhere NSIS Installer Hooks
; Custom uninstall hook to remove app data including media folder

!macro customUnInit
  ; Remove the entire ai-anywhere appdata folder on uninstall
  ; This ensures media files, history, config, and all app data are cleaned up
  
  SetShellVarContext current
  
  ; Remove from Roaming AppData (where config.json and media folder are stored)
  RMDir /r "$APPDATA\ai-anywhere"
  
  ; Remove from Local AppData (in case any cache or temp files exist there)
  RMDir /r "$LOCALAPPDATA\ai-anywhere"
  
  ; Also try the product name variant (with space)
  RMDir /r "$APPDATA\AI Anywhere"
  RMDir /r "$LOCALAPPDATA\AI Anywhere"
  
  ; Try lowercase variant
  RMDir /r "$APPDATA\aianywhere"
  RMDir /r "$LOCALAPPDATA\aianywhere"
!macroend

!macro customInstall
  ; Nothing custom needed on install
!macroend

!macro customUnInstall
  ; Additional cleanup during uninstall process
  ; The customUnInit macro handles the main cleanup
!macroend
