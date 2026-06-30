# PinShot

PinShot 是一个免费的 Windows 截图贴图小工具。

它的目标很简单：按快捷键截图，选中区域后自动复制到剪贴板，并把图片固定在屏幕上。

## 功能

- 区域截图
- 截图后编辑工具栏
- 矩形、圆形、箭头、画笔、文字、马赛克
- 撤销、保存、取消、确认
- 截图后自动复制到剪贴板
- 截图后自动贴到屏幕上
- 贴图窗口置顶
- 鼠标左键拖动贴图
- 鼠标滚轮缩放贴图
- 双击关闭贴图
- 右键菜单：复制、保存、不透明度、关闭
- 托盘菜单
- 可修改截图快捷键

## 默认快捷键

默认截图快捷键是：

```text
Ctrl + Alt + A
```

如果这个快捷键和其他软件冲突：

1. 在系统托盘找到 PinShot 图标
2. 右键图标
3. 点击“设置快捷键”
4. 在输入框里按下新的组合键
5. 点击“保存”

## 适合谁

PinShot 适合只需要截图和贴图的人。

它不做录屏、上传、云同步、OCR、翻译。这样软件更轻，也更容易维护。

## 下载使用

普通用户不需要打开源码。

1. 下载 `PinShot-win-x64.zip`
2. 解压到任意文件夹
3. 双击 `PinShot.exe`
4. 右下角托盘出现 PinShot 图标后，就可以按 `Ctrl + Alt + A` 截图

如果 Windows 提示“未知发布者”，这是因为软件还没有购买代码签名证书。点击“更多信息”，再点“仍要运行”即可。

## 开发运行

### 1. 安装 .NET SDK

请安装 .NET 8 SDK：

https://dotnet.microsoft.com/download/dotnet/8.0

安装完成后，打开 PowerShell，输入：

```powershell
dotnet --version
```

如果能看到版本号，说明安装成功。

### 2. 用 VS Code 打开项目

用 VS Code 打开这个文件夹：

```text
PinShot
```

建议安装 VS Code 扩展：

- C# Dev Kit

### 3. 运行

在 VS Code 里打开终端，运行：

```powershell
dotnet run --project src/PinShot/PinShot.csproj
```

运行后，PinShot 会出现在系统托盘里。

## 如何截图和标注

1. 按 `Ctrl + Alt + A`
2. 鼠标拖动选择截图区域
3. 松开鼠标后会在原屏幕上显示编辑工具栏
4. 可以画框、画圆、画箭头、画笔、文字、马赛克
5. 点 `✓` 确认后，图片会复制到剪贴板并贴到屏幕上

工具栏按钮含义：

```text
□  矩形
○  圆形
↗  箭头
✎  画笔
▦  马赛克
A  文字
↶  撤销
↓  保存
×  取消
✓  确认并贴图
```

## 如何打包 Windows ZIP

在项目根目录运行：

```powershell
dotnet publish src/PinShot/PinShot.csproj -c Release -r win-x64 --self-contained true -o publish
Compress-Archive -Path publish/* -DestinationPath PinShot-win-x64.zip -Force
```

打包后会得到：

```text
PinShot-win-x64.zip
```

这个 zip 是 Windows x64 版本，里面自带 .NET 运行时，解压后双击 `PinShot.exe` 就能用。

## 如何上传到 GitHub

### 方法一：网页上传，适合新手

1. 打开 https://github.com
2. 登录账号
3. 右上角点 `+`
4. 点 `New repository`
5. Repository name 填 `PinShot`
6. 选择 `Public`
7. 不要勾选 `Add a README file`
8. 点 `Create repository`
9. 进入新仓库后，点 `uploading an existing file`
10. 把本项目文件夹里的内容拖进去上传
11. 点 `Commit changes`

建议上传这些内容：

```text
.github/
.vscode/
src/
.gitignore
LICENSE
README.md
```

不要上传这些内容：

```text
bin/
obj/
publish/
release/
*.zip
```

### 方法二：安装 Git 后上传

安装 Git 后，在项目根目录运行：

```powershell
git init
git add .
git commit -m "Initial release"
git branch -M main
git remote add origin https://github.com/你的用户名/PinShot.git
git push -u origin main
```

把 `你的用户名` 换成你的 GitHub 用户名。

## 如何发布正式下载包

上传代码后，GitHub 会自动运行 Actions，并生成 `PinShot-win-x64.zip`。

你可以这样发布给别人下载：

1. 打开 GitHub 仓库
2. 点上方 `Actions`
3. 点最新一次 `Build`
4. 在页面底部下载 `PinShot-win-x64`
5. 回到仓库首页，点右侧 `Releases`
6. 点 `Create a new release`
7. Tag 填 `v1.0.0`
8. Release title 填 `PinShot v1.0.0`
9. 把下载到的 `PinShot-win-x64.zip` 上传到附件
10. 点 `Publish release`

## GitHub 自动构建

这个项目已经包含 GitHub Actions 配置。

你把代码上传到 GitHub 后，每次推送到 `main` 分支，GitHub 会自动构建 Windows 版本，并生成 `PinShot-win-x64.zip` 作为构建产物。

配置文件在：

```text
.github/workflows/build.yml
```

## 许可证

PinShot 使用 MIT License。

你可以免费使用、修改、分发，也可以把它作为自己项目的基础。
