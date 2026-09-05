# UnlockToPhoto - 解锁即拍照

Windows 桌面应用，在系统解锁时自动调用摄像头拍照并保存，支持系统托盘后台运行。

**中文 | [English](README_EN.md) | [日本語](README_JP.md)**

## 软件截图

![设置界面](screenshot/setting.png)

## 功能特性

- **解锁自动拍照** — 监听 Windows 锁屏/解锁事件，解锁后自动拍照
- **系统托盘运行** — 关闭窗口后隐藏至托盘，双击托盘图标打开设置
- **摄像头设备选择** — 支持多摄像头，通过 WMI 获取真实设备名称
- **实时预览** — 设置界面可预览摄像头画面，默认关闭，失焦自动释放
- **开机自启** — 可选注册为开机自动启动
- **自定义保存路径** — 照片按 `年/月/日` 目录分层，文件名格式 `HHmmss.jpg`
- **安装引导** — 提供 Inno Setup 安装包，自动检测 .NET 6 运行时

## 适用场景

### 🔒 电脑安全监控
离开座位时锁定电脑，回来后查看照片，确认是否有人在你不在时动过你的电脑。相比昂贵的物理监控方案，这是一个零成本的软件解决方案。

### 📸 每日自拍记录
开启开机自启后，每次解锁电脑都会自动拍一张照片。日积月累，形成一份独特的"解锁日记"，记录每天的状态和变化。

### 🏢 办公室趣味
同事趁你不在偷偷用你电脑？开启 UnlockToPhoto，下次就能抓到"罪魁祸首"。适合团队间轻松有趣的互动。

### 👨‍👩‍👧 家庭电脑管理
家里的电脑谁在用？安装后开启自启，每次解锁自动拍照，帮助家长了解孩子使用电脑的情况。

### 🕵️ 未授权访问留证
怀疑有人擅自使用你的电脑？照片按日期自动归档，提供时间线索，方便追溯。

### 🎓 培训 / 签到场景
在培训教室或会议室的电脑上运行，参与者解锁屏幕即自动拍照，可作为简易签到记录的辅助手段。

## 技术栈

- .NET 6 / C# / WinForms
- [OpenCvSharp4](https://github.com/shimat/opencvsharp) — 摄像头捕获
- [System.Management](https://www.nuget.org/packages/System.Management) — WMI 设备查询
- [Inno Setup](https://jrsoftware.org/isinfo.php) — 安装包制作

## 项目结构

```
UnlockToPhoto/
├── Program.cs                    # 入口，摄像头检测
├── MainForm.cs                   # 主窗体（隐藏）+ 托盘逻辑
├── MainForm.Designer.cs          # 窗体设计器
── SettingsForm.cs               # 设置界面（含实时预览）
├── UnlockToPhoto.csproj          # 项目文件
├── Models/
│   └── AppSettings.cs            # 配置模型
├── Services/
│   ├── CameraService.cs          # 摄像头枚举、捕获、预览
│   ├── SettingsManager.cs        # JSON 配置读写
│   └── AutoStartManager.cs       # 注册表自启管理
└── icon/
    ├── icon.ico                  # 应用图标
    ├── icon.png                  # 图标源文件
    ├── wizard_sidebar.bmp        # 安装向导侧边图
    ── wizard_header.bmp         # 安装向导顶部图
```

## 快速开始

### 环境要求

- Windows 10/11 (x64)
- [.NET 6 运行时](https://dotnet.microsoft.com/download/dotnet/6.0)

### 从源码编译

```bash
# 克隆仓库
git clone https://github.com/sn-mumu/UnlockToPhoto.git
cd UnlockToPhoto

# 添加 NuGet 源（如未配置）
dotnet nuget add source https://api.nuget.org/v3/index.json

# 编译
dotnet build

# 运行
dotnet run
```

### 发布

```bash
# 框架依赖模式（需目标机器安装 .NET 6）
dotnet publish -c Release -r win-x64 --self-contained false -o publish

# 自包含模式（无需安装 .NET 6，体积较大）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

### 制作安装包

```bash
# 需要先安装 Inno Setup 6
# 编辑 UnlockToPhoto_Setup.iss 中的文件路径
# 编译安装包
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" UnlockToPhoto_Setup.iss
```

## 使用说明

1. 首次运行会检测摄像头，无摄像头则提示退出
2. 程序启动后自动隐藏至系统托盘
3. 双击托盘图标打开设置界面
4. 在设置中选择摄像头设备、保存路径，可开启实时预览
5. 锁定屏幕后再次解锁，程序自动拍照并保存
6. 拍照完成后托盘弹出通知提示

## 配置说明

配置文件保存在 `%APPDATA%\UnlockToPhoto\settings.json`：

```json
{
  "SavePath": "C:\\Users\\<用户名>\\Pictures\\UnlockToPhoto",
  "AutoStart": false,
  "CameraIndex": 0
}
```

| 字段 | 说明 | 默认值 |
|------|------|--------|
| `SavePath` | 照片保存路径 | `Pictures\UnlockToPhoto` |
| `AutoStart` | 开机自启 | `false` |
| `CameraIndex` | 摄像头设备索引 | `0` |

## 许可证

MIT License

## 关键词

`Windows` `解锁拍照` `自动拍照` `摄像头` `监控` `自拍` `安全` `桌面应用` `.NET` `WinForms` `OpenCV` `系统工具` `unlock` `camera` `auto-capture` `webcam` `security` `selfie` `desktop-app`
