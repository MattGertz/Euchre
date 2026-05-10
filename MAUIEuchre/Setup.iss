[Setup]
AppId={{B8A3D4E1-7F2C-4A5D-9E6B-1C3D5F7A8B9E}
AppName=Matt's Euchre
AppVersion=5.1
AppPublisher=Matthew Gertz
DefaultDirName={autopf}\Matts Euchre
DefaultGroupName=Matt's Euchre
UninstallDisplayIcon={app}\Matts Euchre.exe
OutputDir=..\Setup
OutputBaseFilename=MattsEuchreSetup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Matt's Euchre"; Filename: "{app}\Matts Euchre.exe"
Name: "{commondesktop}\Matt's Euchre"; Filename: "{app}\Matts Euchre.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\Matts Euchre.exe"; Description: "Launch Matt's Euchre"; Flags: nowait postinstall skipifsilent
