using System.Management;
using OpenCvSharp;

namespace UnlockToPhoto.Services;

public static class CameraService
{
    // 缓存设备列表，避免每次打开设置都重新枚举（耗时长）
    private static List<int>? _cachedIndices;
    private static List<string>? _cachedNames;

    /// <summary>
    /// 通过 WMI 获取系统摄像头设备名称列表（带缓存）
    /// </summary>
    public static List<string> GetCameraNames()
    {
        if (_cachedNames != null) return _cachedNames;

        _cachedNames = new List<string>();
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"SELECT Name FROM Win32_PnPEntity WHERE PNPClass='Camera' OR PNPClass='Image'");
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name))
                    _cachedNames.Add(name);
            }
        }
        catch
        {
            // WMI 查询失败时返回空列表
        }
        return _cachedNames;
    }

    /// <summary>
    /// 枚举所有可用的摄像头设备（带缓存）
    /// </summary>
    /// <returns>可用摄像头索引列表</returns>
    public static List<int> GetAvailableCameras()
    {
        if (_cachedIndices != null) return _cachedIndices;

        _cachedIndices = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            try
            {
                using var capture = new VideoCapture(i);
                if (capture.IsOpened())
                {
                    _cachedIndices.Add(i);
                }
            }
            catch
            {
                // 跳过无法打开的设备
            }
        }
        return _cachedIndices;
    }

    /// <summary>
    /// 清除缓存，强制下次调用时重新枚举设备
    /// </summary>
    public static void RefreshCache()
    {
        _cachedIndices = null;
        _cachedNames = null;
    }

    /// <summary>
    /// 检测是否有可用摄像头
    /// </summary>
    public static bool IsCameraAvailable()
    {
        return GetAvailableCameras().Count > 0;
    }

    /// <summary>
    /// 使用指定摄像头拍摄照片并保存
    /// </summary>
    /// <param name="baseSavePath">保存根目录</param>
    /// <param name="cameraIndex">摄像头设备索引</param>
    /// <returns>保存的文件路径，失败返回 null</returns>
    public static string? CapturePhoto(string baseSavePath, int cameraIndex = 0)
    {
        try
        {
            var now = DateTime.Now;

            // 创建 年/月/日 分层目录
            var dateDir = Path.Combine(
                baseSavePath,
                now.ToString("yyyy"),
                now.ToString("MM"),
                now.ToString("dd"));
            Directory.CreateDirectory(dateDir);

            // 文件名: hhmmss 格式
            var fileName = $"{now:HHmmss}.jpg";
            var filePath = Path.Combine(dateDir, fileName);

            using var capture = new VideoCapture(cameraIndex);
            if (!capture.IsOpened()) return null;

            // 等待摄像头自动曝光稳定
            using var frame = new Mat();
            // 丢弃前几帧，让摄像头自动调整
            for (int i = 0; i < 5; i++)
            {
                capture.Read(frame);
            }

            if (frame.Empty()) return null;

            Cv2.ImWrite(filePath, frame);
            return filePath;
        }
        catch
        {
            // 拍照失败时静默处理，不弹窗打扰用户
            return null;
        }
    }
}
