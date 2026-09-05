## 安装 Node.js CLI

需要 Node.js 和 npm。根据操作系统和 CPU 架构安装对应的包：

```bash
npm install --global @pcads/crt-cpp-app-win32-x64
```

可用包名为：

- Windows x64：`@pcads/crt-cpp-app-win32-x64`
- Linux x64：`@pcads/crt-cpp-app-linux-x64`
- macOS ARM64：`@pcads/crt-cpp-app-darwin-arm64`

验证安装：

```bash
crt-cpp-app --version
```

## 卸载 Node.js CLI

使用与安装时相同的包名，例如：

```bash
npm uninstall --global @pcads/crt-cpp-app-win32-x64
```
