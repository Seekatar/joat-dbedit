; DbEdit Setup Script for InnoSetup

#pragma option -v+
#pragma verboselevel 9

#define AppName "DbEdit"

#define AppVersion GetFileVersion("Bin\release\DbEdit.exe")

[Setup]
DefaultDirName={pf}\DbEdit
DefaultGroupName=DbEdit
SourceDir=bin\release
LicenseFile={#file AddBackslash(SourcePath) + "License.txt"}
AppName=JOAT {#AppName}
AppVersion={#AppVersion}
ChangesAssociations=yes
UninstallDisplayIcon={app}\{#AppName}.exe

[Files]
Source: {#AppName}.exe; DestDir: "{app}"
Source: ReadMe.rtf; DestDir: "{app}"; Flags: isreadme
;Source: *.dll; DestDir: "{app}"

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppName}.exe"

[Run]
Filename: "{app}\{#AppName}.EXE"; Description: "Launch {#AppName}"; Flags: postinstall nowait skipifsilent unchecked

[Tasks]
Name: mypAssociation; Description: "Associate ""dbecfg"" extension"; GroupDescription: File extensions:

[Registry]
Root: HKCR; Subkey: ".dbecfg"; ValueType: string; ValueName: ""; ValueData: "{app}\DbEdit.exe"; Flags: uninsdeletekey; Tasks: mypAssociation 
Root: HKCR; Subkey: "Applications\DbEdit.exe\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\DbEdit.exe"" ""%1"""; Flags: uninsdeletekey; Tasks: mypAssociation