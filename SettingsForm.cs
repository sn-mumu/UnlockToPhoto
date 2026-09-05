using OpenCvSharp;
using UnlockToPhoto.Models;
using UnlockToPhoto.Services;

namespace UnlockToPhoto;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly ComboBox _cmbLanguage;
    private readonly TextBox _txtSavePath;
    private readonly CheckBox _chkAutoStart;
    private readonly ComboBox _cmbCamera;
    private readonly PictureBox _picPreview;
    private readonly Label _lblPreviewStatus;
    private readonly CheckBox _chkPreview;
    private readonly List<int> _cameraIndices = new();

    // 预览相关
    private VideoCapture? _previewCapture;
    private System.Windows.Forms.Timer? _previewTimer;

    public SettingsForm(AppSettings settings)
    {
        _settings = settings;

        // 应用当前语言
        LocalizationService.SetLanguage(settings.Language);

        // 窗体属性
        Text = LocalizationService.T("SettingsTitle");
        Size = new System.Drawing.Size(500, 595);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 加载自定义图标
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon", "icon.ico");
        if (File.Exists(iconPath))
            Icon = new Icon(iconPath);

        int y = 20;

        // ---- 语言选择（第一栏）----
        var lblLanguage = new Label
        {
            Text = LocalizationService.T("Language"),
            Location = new System.Drawing.Point(20, y),
            AutoSize = true
        };

        _cmbLanguage = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new System.Drawing.Point(110, y - 3),
            Width = 200
        };

        // 填充语言列表
        var langs = LocalizationService.GetSupportedLanguages();
        int langIndex = 0;
        for (int i = 0; i < langs.Length; i++)
        {
            string displayKey = langs[i] switch
            {
                "zh" => "LangZh",
                "en" => "LangEn",
                "ja" => "LangJa",
                _ => "LangZh"
            };
            _cmbLanguage.Items.Add(LocalizationService.T(displayKey));
            if (langs[i] == settings.Language)
                langIndex = i;
        }
        _cmbLanguage.SelectedIndex = langIndex;

        y += 35;

        // ---- 摄像头设备选择 ----
        var lblCamera = new Label
        {
            Text = LocalizationService.T("Camera"),
            Location = new System.Drawing.Point(20, y),
            AutoSize = true
        };

        _cmbCamera = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new System.Drawing.Point(110, y - 3),
            Width = 355
        };

        // 枚举可用摄像头并获取真实名称
        _cameraIndices = CameraService.GetAvailableCameras();
        var cameraNames = CameraService.GetCameraNames();

        if (_cameraIndices.Count == 0)
        {
            _cmbCamera.Items.Add(LocalizationService.T("NoCamera"));
            _cmbCamera.Enabled = false;
        }
        else
        {
            int selectedIndex = 0;
            for (int i = 0; i < _cameraIndices.Count; i++)
            {
                string displayName;
                if (i < cameraNames.Count)
                    displayName = cameraNames[i];
                else
                    displayName = $"{LocalizationService.T("CameraDefault")} {_cameraIndices[i]}";

                _cmbCamera.Items.Add(displayName);

                if (_cameraIndices[i] == settings.CameraIndex)
                    selectedIndex = i;
            }
            _cmbCamera.SelectedIndex = selectedIndex;
        }

        y += 35;

        // ---- 实时预览区域 ----
        _chkPreview = new CheckBox
        {
            Text = LocalizationService.T("EnablePreview"),
            Location = new System.Drawing.Point(20, y),
            AutoSize = true
        };
        _chkPreview.CheckedChanged += OnPreviewToggle;

        _lblPreviewStatus = new Label
        {
            Text = LocalizationService.T("PreviewStatusOff"),
            Location = new System.Drawing.Point(200, y + 2),
            AutoSize = true,
            ForeColor = System.Drawing.Color.Gray
        };

        y += 25;

        _picPreview = new PictureBox
        {
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(445, 250),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = System.Drawing.Color.Black,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        y += 260;

        // ---- 照片保存路径 ----
        var lblSavePath = new Label
        {
            Text = LocalizationService.T("SavePath"),
            Location = new System.Drawing.Point(20, y),
            AutoSize = true
        };

        _txtSavePath = new TextBox
        {
            Text = settings.SavePath,
            Location = new System.Drawing.Point(110, y - 3),
            Width = 265
        };

        var btnBrowse = new Button
        {
            Text = LocalizationService.T("Browse"),
            Location = new System.Drawing.Point(385, y - 4),
            Width = 75
        };
        btnBrowse.Click += OnBrowseClick;

        y += 35;

        // ---- 开机自启动 ----
        _chkAutoStart = new CheckBox
        {
            Text = LocalizationService.T("AutoStart"),
            Location = new System.Drawing.Point(110, y),
            Checked = settings.AutoStart
        };

        y += 40;

        // ---- 按钮 ----
        var btnOk = new Button
        {
            Text = LocalizationService.T("OK"),
            DialogResult = DialogResult.OK,
            Location = new System.Drawing.Point(280, y),
            Width = 85
        };
        btnOk.Click += OnOkClick;

        var btnCancel = new Button
        {
            Text = LocalizationService.T("Cancel"),
            DialogResult = DialogResult.Cancel,
            Location = new System.Drawing.Point(380, y),
            Width = 85
        };

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        Controls.AddRange(new Control[]
        {
            lblLanguage, _cmbLanguage,
            lblCamera, _cmbCamera,
            _chkPreview, _lblPreviewStatus, _picPreview,
            lblSavePath, _txtSavePath, btnBrowse,
            _chkAutoStart, btnOk, btnCancel
        });

        // 所有控件初始化完成后再注册事件，避免构造函数中触发空指针
        _cmbCamera.SelectedIndexChanged += OnCameraChanged;

        // 窗口失去焦点时自动关闭预览
        Deactivate += OnWindowDeactivated;
        FormClosing += OnFormClosing;
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        if (_chkPreview != null)
            _chkPreview.Checked = false;
    }

    private void OnPreviewToggle(object? sender, EventArgs e)
    {
        if (_lblPreviewStatus == null || _picPreview == null) return;

        if (_chkPreview.Checked)
        {
            StartPreview();
        }
        else
        {
            StopPreview();
            _lblPreviewStatus.Text = LocalizationService.T("PreviewStatusOff");
        }
    }

    private void OnCameraChanged(object? sender, EventArgs e)
    {
        if (_lblPreviewStatus == null || _picPreview == null) return;

        if (_chkPreview != null && _chkPreview.Checked)
        {
            StopPreview();
            StartPreview();
        }
    }

    private void StartPreview()
    {
        if (_lblPreviewStatus == null) return;

        if (_cameraIndices.Count == 0 || _cmbCamera.SelectedIndex < 0)
        {
            _lblPreviewStatus.Text = LocalizationService.T("NoDevice");
            return;
        }

        var cameraIndex = _cameraIndices[_cmbCamera.SelectedIndex];

        try
        {
            _previewCapture?.Dispose();
            _previewCapture = new VideoCapture(cameraIndex);

            if (!_previewCapture.IsOpened())
            {
                _lblPreviewStatus.Text = LocalizationService.T("OpenFailed");
                _previewCapture.Dispose();
                _previewCapture = null;
                return;
            }

            _lblPreviewStatus.Text = LocalizationService.T("PreviewStatusOn");

            _previewTimer?.Dispose();
            _previewTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _previewTimer.Tick += OnPreviewTick;
            _previewTimer.Start();
        }
        catch
        {
            _lblPreviewStatus.Text = LocalizationService.T("PreviewFailed");
        }
    }

    private void StopPreview()
    {
        _previewTimer?.Stop();
        _previewTimer?.Dispose();
        _previewTimer = null;

        _previewCapture?.Release();
        _previewCapture?.Dispose();
        _previewCapture = null;

        if (_picPreview != null)
            _picPreview.Image = null;
        if (_lblPreviewStatus != null)
            _lblPreviewStatus.Text = "";
    }

    private void OnPreviewTick(object? sender, EventArgs e)
    {
        if (_previewCapture == null || !_previewCapture.IsOpened()) return;

        try
        {
            using var frame = new Mat();
            if (_previewCapture.Read(frame) && !frame.Empty())
            {
                var oldImage = _picPreview.Image;
                _picPreview.Image = MatToBitmap(frame);
                oldImage?.Dispose();
            }
        }
        catch
        {
            // 预览帧读取失败，静默处理
        }
    }

    private static System.Drawing.Bitmap MatToBitmap(Mat mat)
    {
        if (mat.Channels() == 1)
        {
            var bmp = new System.Drawing.Bitmap(
                mat.Cols, mat.Rows,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var bmpData = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                bmp.PixelFormat);
            unsafe
            {
                for (int row = 0; row < mat.Rows; row++)
                {
                    System.Buffer.MemoryCopy(
                        (byte*)mat.Data + row * mat.Step(),
                        (byte*)bmpData.Scan0 + row * bmpData.Stride,
                        bmpData.Stride, mat.Cols);
                }
            }
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        using var rgb = new Mat();
        Cv2.CvtColor(mat, rgb, ColorConversionCodes.BGR2RGB);
        var bmpRgb = new System.Drawing.Bitmap(
            rgb.Cols, rgb.Rows,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        var rgbData = bmpRgb.LockBits(
            new System.Drawing.Rectangle(0, 0, bmpRgb.Width, bmpRgb.Height),
            System.Drawing.Imaging.ImageLockMode.WriteOnly,
            bmpRgb.PixelFormat);
        unsafe
        {
            for (int row = 0; row < rgb.Rows; row++)
            {
                System.Buffer.MemoryCopy(
                    (byte*)rgb.Data + row * rgb.Step(),
                    (byte*)rgbData.Scan0 + row * rgbData.Stride,
                    rgbData.Stride, rgb.Cols * 3);
            }
        }
        bmpRgb.UnlockBits(rgbData);
        return bmpRgb;
    }

    private void OnBrowseClick(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            SelectedPath = _txtSavePath.Text,
            Description = LocalizationService.T("SelectFolder")
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _txtSavePath.Text = dialog.SelectedPath;
        }
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        var originalLanguage = _settings.Language;

        var newSavePath = _txtSavePath.Text.Trim();
        if (string.IsNullOrEmpty(newSavePath))
        {
            MessageBox.Show(
                LocalizationService.T("SavePathEmpty"),
                LocalizationService.T("Tip"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        // 保存语言设置
        var langs = LocalizationService.GetSupportedLanguages();
        if (_cmbLanguage.SelectedIndex >= 0 && _cmbLanguage.SelectedIndex < langs.Length)
        {
            _settings.Language = langs[_cmbLanguage.SelectedIndex];
        }

        _settings.SavePath = newSavePath;
        _settings.AutoStart = _chkAutoStart.Checked;

        if (_cameraIndices.Count > 0 && _cmbCamera.SelectedIndex >= 0)
        {
            _settings.CameraIndex = _cameraIndices[_cmbCamera.SelectedIndex];
        }

        SettingsManager.Save(_settings);
        AutoStartManager.SetAutoStart(_settings.AutoStart);

        // 语言变更提示
        if (_cmbLanguage.SelectedIndex >= 0 && _cmbLanguage.SelectedIndex < langs.Length)
        {
            var newLang = langs[_cmbLanguage.SelectedIndex];
            if (newLang != originalLanguage)
            {
                MessageBox.Show(
                    LocalizationService.T("RestartRequired"),
                    LocalizationService.T("Tip"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        Close();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        StopPreview();
    }
}
