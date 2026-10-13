; CodeWF.Toolkit Demo Windows installer.
; Build from the repository root with Inno Setup 6 and pass /DAppVersion=x.y.z.

#ifndef AppVersion
#define AppVersion "0.0.0"
#endif

#ifndef SourceDir
#define SourceDir "..\artifacts\publish\win-x64\CodeWF.Toolkit.Demo.Avalonia"
#endif

#ifndef OutputDir
#define OutputDir "..\artifacts\release"
#endif

[Setup]
AppId={{2A5F7E14-9C0B-4D3A-8E62-51B7C9D0A4F8}
AppName=CodeWF.Toolkit Demo
AppVersion={#AppVersion}
AppPublisher=Dotnet9
AppPublisherURL=https://github.com/dotnet9/CodeWF.Toolkit
AppSupportURL=https://github.com/dotnet9/CodeWF.Toolkit/issues
DefaultDirName={autopf}\CodeWF.Toolkit Demo
DefaultGroupName=CodeWF.Toolkit Demo
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=CodeWF.Toolkit.Demo-v{#AppVersion}-win-x64-setup
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; per-user 安装：演示应用以普通权限运行，日志写入用户临时目录。
PrivilegesRequired=lowest
ChangesAssociations=no
CloseApplications=yes
RestartApplications=yes
CloseApplicationsFilter=CodeWF.Toolkit.Demo.Avalonia.exe
UninstallDisplayIcon={app}\CodeWF.Toolkit.Demo.Avalonia.exe
WizardStyle=modern

[Languages]
Name: "chinesesimplified"; MessagesFile: "Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\CodeWF.Toolkit Demo"; Filename: "{app}\CodeWF.Toolkit.Demo.Avalonia.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\CodeWF.Toolkit Demo"; Filename: "{app}\CodeWF.Toolkit.Demo.Avalonia.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\CodeWF.Toolkit.Demo.Avalonia.exe"; Description: "Launch CodeWF.Toolkit Demo"; Flags: nowait postinstall skipifsilent
