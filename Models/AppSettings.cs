using System.Text.Json.Serialization;

namespace UnlockToPhoto.Models;

public class AppSettings
{
    /// <summary>
    /// 界面语言：zh（中文）、en（English）、ja（日本語）
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = "zh";

    /// <summary>
    /// 照片保存根目录，默认为 Windows 图片文件夹下的 UnlockToPhoto 子目录
    /// </summary>
    [JsonPropertyName("savePath")]
    public string SavePath { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "UnlockToPhoto");

    /// <summary>
    /// 是否开机自启动
    /// </summary>
    [JsonPropertyName("autoStart")]
    public bool AutoStart { get; set; } = false;

    /// <summary>
    /// 选中的摄像头设备索引，默认 0
    /// </summary>
    [JsonPropertyName("cameraIndex")]
    public int CameraIndex { get; set; } = 0;
}
