#define MyAppName "Quan Ly Nhan Su WPF"
#define MyAppExeName "QuanLyNhanSuWpf.exe"
#define MyAppVersion "1.0.0"

[Setup]
AppId={{21B68E13-D0C6-4D32-B9F4-2E7128D93789}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\QuanLyNhanSuWpf
DefaultGroupName={#MyAppName}
OutputDir=..\artifacts\installer
OutputBaseFilename=QuanLyNhanSuWpf-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "..\artifacts\QuanLyNhanSuWpf\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
