; Script de Inno Setup para el instalador de Fortnite Launcher Ligero.
; Compilar con: iscc setup.iss
; Toma los archivos publicados en bin\Release\net8.0-windows\win-x64\publish\
; (borrar cualquier .pdb o .zip viejo de esa carpeta antes de compilar).

#define MyAppName "Fortnite Launcher Ligero"
#define MyAppVersion "1.2.0"
#define MyAppExeName "FortniteLauncher.exe"
#define MyAppPublisher "Mateo"

[Setup]
AppId={{4C9F2A1E-7B3D-4F6A-8E2C-1A5D9B6E3C70}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\bin\installer
OutputBaseFilename=FortniteLiteLauncher-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos:"

[Files]
Source: "..\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir {#MyAppName}"; Flags: nowait postinstall skipifsilent
