using Microsoft.Win32;

namespace UnlockToPhoto.Services;

public static class AutoStartManager
{
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "UnlockToPhoto";

    /// <summary>
    /// 设置是否开机自启动
    /// </summary>
    public static void SetAutoStart(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
        if (key == null) return;

        if (enable)
        {
            var exePath = Application.ExecutablePath;
            key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(AppName, false);
        }
    }

    /// <summary>
    /// 检查是否已设置开机自启动
    /// </summary>
    public static bool IsAutoStart()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
        if (key == null) return false;

        var value = key.GetValue(AppName);
        return value != null;
    }
}
