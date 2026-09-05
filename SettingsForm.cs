using OpenCvSharp;
using UnlockToPhoto.Models;
using UnlockToPhoto.Services;

namespace UnlockToPhoto;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;
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

        // 窗体属性
        Text = "解锁即拍照 - 设置";
        Size = new System.Drawing.Size(500, 560);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 加载自定义图标
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon", "icon.ico");
        if (File.Exists(iconPath))
            Icon = new Icon(iconPath);

        int y = 20;

        // ---- 摄像头设备选择 ----
        var lblCamera = new Label
        {
            Text = "摄像头设备:",
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
            _cmbCamera.Items.Add("（未检测到摄像头）");
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
                    displayName = $"摄像头设备 {_cameraIndices[i]}";

                _cmbCamera.Items.Add(displayName);

                if (_cameraIndices[i] == settings.CameraIndex)
                    selectedIndex = i;
            }
            _cmbCamera.SelectedIndex = selectedIndex;
        }

        y += 35;

        // ---- 实时预览区域 ----
        var lblPreview = new Label
        {
            Text = "实时预览:",
            Location = new System.Drawing.Point(20, y),
            AutoSize = true
        };

        _chkPreview = new CheckBox
        {
            Text = "开启",
            Location = new System.Drawing.Point(80, y - 2),
            AutoSize = true
        };
        _chkPreview.CheckedChanged += OnPreviewToggle;

        _lblPreviewStatus = new Label
        {
            Text = "已关闭",
            Location = new System.Drawing.Point(130, y),
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
            Text = "保存路径:",
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
            Text = "浏览...",
            Location = new System.Drawing.Point(385, y - 4),
            Width = 75
        };
        btnBrowse.Click += OnBrowseClick;

        y += 35;

        // ---- 开机自启动 ----
        _chkAutoStart = new CheckBox
        {
            Text = "开机自动启动",
            Location = new System.Drawing.Point(110, y),
            Checked = settings.AutoStart
        };

        y += 40;

        // ---- 按钮 ----
        var btnOk = new Button
        {
            Text = "确定",
            DialogResult = DialogResult.OK,
            Location = new System.Drawing.Point(280, y),
            Width = 85
        };
        btnOk.Click += OnOkClick;

        var btnCancel = new Button
        {
            Text = "取消",
            DialogResult = DialogResult.Cancel,
            Location = new System.Drawing.Point(380, y),
            Width = 85
        };

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        Controls.AddRange(new Control[]
        {
            lblCamera, _cmbCamera,
            lblPreview, _chkPreview, _lblPreviewStatus, _picPreview,
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
            _lblPreviewStatus.Text = "已关闭";
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
            _lblPreviewStatus.Text = "无可用设备";
            return;
        }

        var cameraIndex = _cameraIndices[_cmbCamera.SelectedIndex];

        try
        {
            _previewCapture?.Dispose();
            _previewCapture = new VideoCapture(cameraIndex);

            if (!_previewCapture.IsOpened())
            {
                _lblPreviewStatus.Text = "无法打开设备";
                _previewCapture.Dispose();
                _previewCapture = null;
                return;
            }

            _lblPreviewStatus.Text = "预览中...";

            _previewTimer?.Dispose();
            _previewTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _previewTimer.Tick += OnPreviewTick;
            _previewTimer.Start();
        }
        catch
        {
            _lblPreviewStatus.Text = "预览失败";
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
            Description = "选择照片保存目录"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _txtSavePath.Text = dialog.SelectedPath;
        }
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        var newSavePath = _txtSavePath.Text.Trim();
        if (string.IsNullOrEmpty(newSavePath))
        {
            MessageBox.Show("保存路径不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        _settings.SavePath = newSavePath;
        _settings.AutoStart = _chkAutoStart.Checked;

        if (_cameraIndices.Count > 0 && _cmbCamera.SelectedIndex >= 0)
        {
            _settings.CameraIndex = _cameraIndices[_cmbCamera.SelectedIndex];
        }

        SettingsManager.Save(_settings);
        AutoStartManager.SetAutoStart(_settings.AutoStart);

        Close();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        StopPreview();
    }
}
