## 安装 NuGet CLI

需要 .NET SDK。安装 .NET 全局工具：

```bash
dotnet tool install --global crt-cpp-app
```

如果只希望安装到指定目录，可以使用 `--tool-path`：

```bash
dotnet tool install --tool-path <目录> crt-cpp-app
```

验证安装：

```bash
crt-cpp-app --version
```

## 卸载 NuGet CLI

全局安装的工具：

```bash
dotnet tool uninstall --global crt-cpp-app
```

指定目录安装的工具：

```bash
dotnet tool uninstall --tool-path <目录> crt-cpp-app
```
