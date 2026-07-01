# PinShot

PinShot is a lightweight Windows screenshot and screen-pinning tool.

It is made for quick screenshots, simple annotations, and keeping captured images pinned on top of your desktop.

## Download

Download the latest Windows ZIP from Releases:

https://github.com/crazyxw6/PinShot/releases/latest

Use it like a normal portable Windows app:

1. Download `PinShot-win-x64.zip`.
2. Unzip it to any folder.
3. Run `PinShot.exe`.
4. Use `Ctrl + Alt + A` to start a screenshot.

No .NET SDK installation is required.

## Features

- Region screenshot
- Pin screenshots on screen
- Copy screenshots to clipboard
- Simple annotation toolbar
- Rectangle, ellipse, arrow, pen, text, blur/mosaic
- Save image
- Tray icon
- Custom screenshot hotkey
- Move and resize the selected capture area
- Move and resize pinned images

## Default Shortcut

`Ctrl + Alt + A`

You can change the shortcut from the tray icon menu.

## How To Use

1. Run `PinShot.exe`.
2. Find the PinShot icon in the Windows system tray.
3. Press `Ctrl + Alt + A`.
4. Drag to select a screen area.
5. Annotate if needed.
6. Click the check button to copy and pin the screenshot.

## Notes

Windows may show a SmartScreen warning because PinShot is not code-signed yet.

If you trust this release, click `More info`, then click `Run anyway`.

## Build From Source

This section is only for developers.

Requirements:

- .NET 8 SDK
- Windows

Build a portable Windows ZIP:

```powershell
dotnet publish src/PinShot/PinShot.csproj -c Release -r win-x64 --self-contained true -o publish
Compress-Archive -Path publish/* -DestinationPath PinShot-win-x64.zip -Force
```

GitHub Actions also builds `PinShot-win-x64.zip` automatically on pushes to `main`.

## 中文说明

普通用户请直接下载 Releases 里的 `PinShot-win-x64.zip`，解压后双击 `PinShot.exe` 使用。

不需要安装 .NET SDK，也不需要安装 VS Code。

## License

MIT License.
