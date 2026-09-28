#define MyAppName "Khmer Astrology"
#define MyAppVersion "1.0.0"
#define MyAppExeName "KhmerAstrology.WinForms.exe"

[Setup]
AppId={{8528109E-62D5-47A1-9E24-2BA8A4F33871}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=KhmerAstrology-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
SetupIconFile=src\KhmerAstrology.WinForms\app.ico
WizardImageFile=src\KhmerAstrology.WinForms\app-logo.png
WizardSmallImageFile=src\KhmerAstrology.WinForms\app-logo.png

[Files]
Source: "src\KhmerAstrology.WinForms\bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
