#define MyAppName "解锁即拍照"
#define MyAppNameEn "UnlockToPhoto"
#define MyAppVersion "0.01.2"
#define MyAppPublisher "sn-mumu"
#define MyAppExeName "UnlockToPhoto.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppNameEn}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=installer
OutputBaseFilename=UnlockToPhoto_Setup_v{#MyAppVersion}
SetupIconFile=icon\icon.ico
WizardImageFile=icon\wizard_sidebar.bmp
WizardSmallImageFile=icon\wizard_header.bmp
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\icon\icon.ico
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\build\UnlockToPhoto.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\UnlockToPhoto.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\UnlockToPhoto.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\UnlockToPhoto.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\OpenCvSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\OpenCvSharpExtern.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\opencv_videoio_ffmpeg4130_64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\System.Management.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\build\icon\*"; DestDir: "{app}\icon"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// 检测 .NET 6 运行时
function IsDotNet6Installed(): Boolean;
var
  installed: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App', '6.', installed) and (installed = 1);
  if not Result then
    Result := RegQueryDWordValue(HKLM32, 'SOFTWARE\dotnet\Setup\InstalledVersions\x86\sharedfx\Microsoft.NETCore.App', '6.', installed) and (installed = 1);
end;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  if not IsDotNet6Installed() then
  begin
    MsgBox('您的电脑尚未安装 .NET 6 运行时。' + #13#10 + #13#10 +
           '即将打开下载页面，请安装后重新运行本安装程序。' + #13#10 + #13#10 +
           '下载地址: https://dotnet.microsoft.com/download/dotnet/6.0',
           mbInformation, MB_OK);
    ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/6.0', '', '', SW_SHOW, ewNoWait, ErrorCode);
    Result := False;
  end
  else
    Result := True;
end;
