using UnlockToPhoto.Models;
using UnlockToPhoto.Services;

namespace UnlockToPhoto;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 加载设置并应用语言
        var settings = SettingsManager.Load();
        LocalizationService.SetLanguage(settings.Language);

        // 检测摄像头是否可用
        if (!CameraService.IsCameraAvailable())
        {
            MessageBox.Show(
                LocalizationService.T("NoCameraMsg"),
                LocalizationService.T("NoCameraTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        Application.Run(new MainForm());
    }
}
