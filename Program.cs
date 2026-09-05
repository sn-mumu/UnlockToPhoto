using UnlockToPhoto.Services;

namespace UnlockToPhoto;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 检测摄像头是否可用
        if (!CameraService.IsCameraAvailable())
        {
            MessageBox.Show(
                "未检测到可用摄像头！\n\n请确保摄像头已正确连接后重试。",
                "解锁即拍照",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        Application.Run(new MainForm());
    }
}
