@echo off
chcp 65001 >nul 2>&1
title 解锁即拍照

:: 检测 .NET 6 运行时是否已安装
reg query "HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App" /v 6. >nul 2>&1
if %errorlevel% equ 0 (
    :: 也检查 x86 注册表视图（兼容 32 位系统）
    goto :launch
)
reg query "HKLM\SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x86\sharedfx\Microsoft.NETCore.App" /v 6. >nul 2>&1
if %errorlevel% equ 0 goto :launch

:: 未检测到 .NET 6，引导用户安装
echo.
echo  ============================================
echo   解锁即拍照 - 需要 .NET 6 运行环境
echo  ============================================
echo.
echo  您的电脑尚未安装 .NET 6 运行时。
echo  即将打开下载页面，请安装后重新运行本程序。
echo.
echo  下载地址: https://dotnet.microsoft.com/download/dotnet/6.0
echo.
pause
start "" "https://dotnet.microsoft.com/download/dotnet/6.0"
exit /b

:launch
start "" "%~dp0UnlockToPhoto.exe"
exit /b
