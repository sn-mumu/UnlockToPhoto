namespace UnlockToPhoto.Services;

/// <summary>
/// 多语言本地化服务，支持中文、英文、日文
/// </summary>
public static class LocalizationService
{
    private static string _currentLanguage = "zh";

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        // ==================== 中文 ====================
        ["zh"] = new()
        {
            // 通用
            ["AppName"] = "解锁即拍照",
            ["OK"] = "确定",
            ["Cancel"] = "取消",
            ["Browse"] = "浏览...",
            ["Tip"] = "提示",
            ["Warning"] = "警告",

            // 设置窗口
            ["SettingsTitle"] = "解锁即拍照 - 设置",
            ["Language"] = "语言:",
            ["RestartRequired"] = "语言切换将在下次启动时生效",
            ["Camera"] = "摄像头:",
            ["EnablePreview"] = "开启实时预览",
            ["PreviewStatusOff"] = "已关闭",
            ["PreviewStatusOn"] = "预览中...",
            ["NoDevice"] = "无可用设备",
            ["OpenFailed"] = "无法打开设备",
            ["PreviewFailed"] = "预览失败",
            ["SavePath"] = "保存路径:",
            ["AutoStart"] = "开机自动启动",
            ["NoCamera"] = "（未检测到摄像头）",
            ["CameraDefault"] = "摄像头",
            ["SavePathEmpty"] = "保存路径不能为空",
            ["SelectFolder"] = "选择照片保存目录",

            // 主窗口 / 托盘
            ["TrayRunning"] = "解锁即拍照 - 运行中",
            ["TraySettings"] = "设置",
            ["TrayExit"] = "退出",
            ["PhotoSaved"] = "照片已保存: {0}",

            // 启动检测
            ["NoCameraTitle"] = "未检测到摄像头",
            ["NoCameraMsg"] = "未检测到可用摄像头！\n\n请确保摄像头已正确连接后重试。",

            // 语言名称
            ["LangZh"] = "中文",
            ["LangEn"] = "English",
            ["LangJa"] = "日本語",
        },

        // ==================== English ====================
        ["en"] = new()
        {
            // General
            ["AppName"] = "UnlockToPhoto",
            ["OK"] = "OK",
            ["Cancel"] = "Cancel",
            ["Browse"] = "Browse...",
            ["Tip"] = "Notice",
            ["Warning"] = "Warning",

            // Settings
            ["SettingsTitle"] = "UnlockToPhoto - Settings",
            ["Language"] = "Language:",
            ["RestartRequired"] = "Language change will take effect on next launch",
            ["Camera"] = "Camera:",
            ["EnablePreview"] = "Enable live preview",
            ["PreviewStatusOff"] = "Off",
            ["PreviewStatusOn"] = "Previewing...",
            ["NoDevice"] = "No device available",
            ["OpenFailed"] = "Failed to open device",
            ["PreviewFailed"] = "Preview failed",
            ["SavePath"] = "Save path:",
            ["AutoStart"] = "Launch on system startup",
            ["NoCamera"] = "(No camera detected)",
            ["CameraDefault"] = "Camera",
            ["SavePathEmpty"] = "Save path cannot be empty",
            ["SelectFolder"] = "Select photo save directory",

            // Main / Tray
            ["TrayRunning"] = "UnlockToPhoto - Running",
            ["TraySettings"] = "Settings",
            ["TrayExit"] = "Exit",
            ["PhotoSaved"] = "Photo saved: {0}",

            // Startup
            ["NoCameraTitle"] = "No Camera Detected",
            ["NoCameraMsg"] = "No available camera detected!\n\nPlease make sure the camera is properly connected and try again.",

            // Language names
            ["LangZh"] = "中文",
            ["LangEn"] = "English",
            ["LangJa"] = "日本語",
        },

        // ==================== 日本語 ====================
        ["ja"] = new()
        {
            // 共通
            ["AppName"] = "UnlockToPhoto",
            ["OK"] = "OK",
            ["Cancel"] = "キャンセル",
            ["Browse"] = "参照...",
            ["Tip"] = "お知らせ",
            ["Warning"] = "警告",

            // 設定画面
            ["SettingsTitle"] = "UnlockToPhoto - 設定",
            ["Language"] = "言語:",
            ["RestartRequired"] = "言語の変更は次回起動時に適用されます",
            ["Camera"] = "カメラ:",
            ["EnablePreview"] = "ライブプレビューを有効にする",
            ["PreviewStatusOff"] = "オフ",
            ["PreviewStatusOn"] = "プレビュー中...",
            ["NoDevice"] = "利用可能なデバイスがありません",
            ["OpenFailed"] = "デバイスを開けませんでした",
            ["PreviewFailed"] = "プレビューに失敗しました",
            ["SavePath"] = "保存パス:",
            ["AutoStart"] = "システム起動時に自動実行",
            ["NoCamera"] = "（カメラが検出されません）",
            ["CameraDefault"] = "カメラ",
            ["SavePathEmpty"] = "保存パスを入力してください",
            ["SelectFolder"] = "写真の保存先フォルダを選択",

            // メイン / トレイ
            ["TrayRunning"] = "UnlockToPhoto - 実行中",
            ["TraySettings"] = "設定",
            ["TrayExit"] = "終了",
            ["PhotoSaved"] = "写真を保存しました: {0}",

            // 起動検出
            ["NoCameraTitle"] = "カメラが検出されません",
            ["NoCameraMsg"] = "利用可能なカメラが検出されませんでした！\n\nカメラが正しく接続されているか確認してください。",

            // 言語名
            ["LangZh"] = "中文",
            ["LangEn"] = "English",
            ["LangJa"] = "日本語",
        },
    };

    /// <summary>
    /// 设置当前语言
    /// </summary>
    public static void SetLanguage(string lang)
    {
        if (Strings.ContainsKey(lang))
            _currentLanguage = lang;
    }

    /// <summary>
    /// 获取当前语言代码
    /// </summary>
    public static string GetLanguage() => _currentLanguage;

    /// <summary>
    /// 获取本地化字符串，支持 {0} {1} 格式化参数
    /// </summary>
    public static string T(string key, params object[] args)
    {
        if (Strings.TryGetValue(_currentLanguage, out var dict) && dict.TryGetValue(key, out var value))
        {
            return args.Length > 0 ? string.Format(value, args) : value;
        }

        // 回退到中文
        if (Strings["zh"].TryGetValue(key, out var fallback))
        {
            return args.Length > 0 ? string.Format(fallback, args) : fallback;
        }

        return key;
    }

    /// <summary>
    /// 获取支持的语言列表
    /// </summary>
    public static string[] GetSupportedLanguages() => new[] { "zh", "en", "ja" };
}
