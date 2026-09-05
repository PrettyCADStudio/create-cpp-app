## 安装本地应用

从发布页下载与操作系统和 CPU 架构匹配的 ZIP 包并解压。将解压目录加入用户 `PATH`，然后重新打开终端。

也可以使用发布包中的安装脚本：

```bash
python install.py --source <解压后的发布目录>
```

验证安装：

```bash
crt-cpp-app --version
```

本地应用是自包含发布，不需要安装 .NET runtime 或 SDK。

## 卸载本地应用

删除安装目录 `~/.crt-cpp-app`，并从用户 `PATH` 中移除该目录。

Windows PowerShell 示例：

```powershell
Remove-Item "$HOME\.crt-cpp-app" -Recurse -Force
```

Linux 和 macOS 示例：

```bash
rm -rf "$HOME/.crt-cpp-app"
```
