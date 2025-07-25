﻿; https://github.com/DomGries/InnoDependencyInstaller


; requires dxwebsetup.exe (see CodeDependencies.iss)
;#define public Dependency_Path_DirectX "dependencies\"

#include "CodeDependencies.iss"

[Setup]
#define MyAppSetupName 'AI Anywhere'
#define MyAppVersion '1.1.3'
#define MyAppPublisher 'LABiA-FUP/UnB'
#define MyAppCopyright 'Copyright © LABiA-FUP/UnB'
#define MyAppURL 'https://github.com/bgeneto/AIAnywhere'
#define AppDir 'AIAnywhere-Optimized'

AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright={#MyAppCopyright}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
OutputBaseFilename={#MyAppSetupName}-{#MyAppVersion}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={autopf}\{#MyAppSetupName}
UninstallDisplayIcon={app}\MyProgram.exe
OutputDir={#SourcePath}\bin
AllowNoIcons=no
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; remove next line if you only deploy 32-bit binaries and dependencies
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: fr; MessagesFile: "compiler:Languages\French.isl"
Name: it; MessagesFile: "compiler:Languages\Italian.isl"
Name: de; MessagesFile: "compiler:Languages\German.isl"
Name: es; MessagesFile: "compiler:Languages\Spanish.isl"
Name: pt; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Files]
Source: "{#AppDir}\AIAnywhere.exe"; DestDir: "{app}"; DestName: "AIAnywhere.exe"; Check: Dependency_IsX64; Flags: ignoreversion
Source: "{#AppDir}\AIAnywhere.exe"; DestDir: "{app}"; Check: not Dependency_IsX64; Flags: ignoreversion
Source: "{#AppDir}\README.md"; DestDir: "{app}"; Flags: isreadme
Source: "{#AppDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppDir}\*.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppDir}\AIAnywhere.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppDir}\AIAnywhere.png"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\AIAnywhere.exe"; IconFilename: "{#AppDir}\AIAnywhere.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppSetupName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppSetupName}"; Filename: "{app}\AIAnywhere.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"

[Run]
Filename: "{app}\AIAnywhere.exe"; Description: "{cm:LaunchProgram,{#MyAppSetupName}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup: Boolean;
begin
  // comment out functions to disable installing them
  Dependency_ForceX86 := False; // disable forced 32-bit install again
  //Dependency_AddDotNet35;
  //Dependency_AddDotNet40;
  //Dependency_AddDotNet45;
  //Dependency_AddDotNet46;
  //Dependency_AddDotNet47;
  //Dependency_AddDotNet48;
  //Dependency_AddDotNet481;

  //Dependency_AddNetCore31;
  //Dependency_AddNetCore31Asp;
  // Dependency_AddNetCore31Desktop;
  // Dependency_AddDotNet50;
  // Dependency_AddDotNet50Asp;
  // Dependency_AddDotNet50Desktop;
  // Dependency_AddDotNet60;
  // Dependency_AddDotNet60Asp;
  // Dependency_AddDotNet60Desktop;
  // Dependency_AddDotNet70;
  // Dependency_AddDotNet70Asp;
  // Dependency_AddDotNet70Desktop;
  // Dependency_AddDotNet80;
  // Dependency_AddDotNet80Asp;
  // Dependency_AddDotNet80Desktop;
  // Dependency_AddDotNet90;
  // Dependency_AddDotNet90Asp;
  Dependency_AddDotNet90Desktop;

  //Dependency_AddVC2005;
  //Dependency_AddVC2008;
  //Dependency_AddVC2010;
  //Dependency_AddVC2012;
  //Dependency_ForceX86 := True; // force 32-bit install of next dependencies
  //Dependency_AddVC2013;
  //Dependency_ForceX86 := False; // disable forced 32-bit install again
  //Dependency_AddVC2015To2022;

  //Dependency_AddDirectX;

  // Dependency_AddSql2008Express;
  // Dependency_AddSql2012Express;
  // Dependency_AddSql2014Express;
  // Dependency_AddSql2016Express;
  // Dependency_AddSql2017Express;
  // Dependency_AddSql2019Express;
  // Dependency_AddSql2022Express;

  // Dependency_AddWebView2;

  // Dependency_AddAccessDatabaseEngine2010;
  // Dependency_AddAccessDatabaseEngine2016;

  Result := True;
end;
