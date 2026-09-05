using Microsoft.Win32;
using UnlockToPhoto.Models;
using UnlockToPhoto.Services;

namespace UnlockToPhoto;

public partial class MainForm : Form
{
    private readonly NotifyIcon _trayIcon;
    private readonly AppSettings _settings;
    private bool _isExiting = false;

    public MainForm()
    {
        InitializeComponent();

        _settings = SettingsManager.Load();

        // 首次启动时自动注册开机自启
        if (SettingsManager.IsFirstRun && _settings.AutoStart)
        {
            AutoStartManager.SetAutoStart(true);
        }

        // 应用语言设置
        LocalizationService.SetLanguage(_settings.Language);

        // 加载自定义图标
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon", "icon.ico");
        Icon? appIcon = null;
        if (File.Exists(iconPath))
            appIcon = new Icon(iconPath);

        if (appIcon != null)
        {
            Icon = appIcon;
        }

        // 初始化系统托盘图标
        _trayIcon = new NotifyIcon
        {
            Icon = appIcon ?? SystemIcons.Application,
            Text = LocalizationService.T("TrayRunning"),
            Visible = true
        };

        // 托盘右键菜单
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(LocalizationService.T("TraySettings"), null, OnSettingsClick);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(LocalizationService.T("TrayExit"), null, OnExitClick);
        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += OnSettingsClick;

        // 监听 Windows 会话切换事件（锁屏/解锁）
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionUnlock)
        {
            // 延迟 2 秒再拍照，让用户回到座位，拍到正脸
            Task.Delay(2000).ContinueWith(_ =>
            {
                var result = CameraService.CapturePhoto(_settings.SavePath, _settings.CameraIndex);
                if (result != null)
                {
                    // 更新托盘提示
                    _trayIcon.ShowBalloonTip(
                        3000,
                        LocalizationService.T("AppName"),
                        LocalizationService.T("PhotoSaved", Path.GetFileName(result)),
                        ToolTipIcon.Info);
                }
            });
        }
    }

    private void OnSettingsClick(object? sender, EventArgs e)
    {
        var settingsForm = new SettingsForm(_settings);
        settingsForm.ShowDialog();
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        _isExiting = true;
        _trayIcon.Visible = false;
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        Application.Exit();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_isExiting)
        {
            // 点击关闭按钮时隐藏到托盘，而非退出
            e.Cancel = true;
            Hide();
        }
        else
        {
            _trayIcon.Dispose();
            base.OnFormClosing(e);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // 启动后隐藏主窗体，只显示托盘图标
        Hide();
    }
}
