using System.Text.Json;
using UnlockToPhoto.Models;

namespace UnlockToPhoto.Services;

public static class SettingsManager
{
    private static readonly string SettingsFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// 是否为首次运行（配置文件不存在时创建）
    /// </summary>
    public static bool IsFirstRun { get; private set; }

    /// <summary>
    /// 加载设置，若配置文件不存在则创建默认设置
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                IsFirstRun = false;
                return settings ?? CreateDefault();
            }
        }
        catch
        {
            // 配置文件损坏，重新创建默认设置
        }

        IsFirstRun = true;
        return CreateDefault();
    }

    /// <summary>
    /// 保存设置到 JSON 文件
    /// </summary>
    public static void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
    }

    private static AppSettings CreateDefault()
    {
        var settings = new AppSettings();
        Save(settings);
        return settings;
    }
}
