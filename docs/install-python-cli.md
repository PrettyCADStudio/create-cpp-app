## 安装 Python CLI

需要 Python 3.9 或更高版本。使用 pip 安装：

```bash
pip install crt-cpp-app
```

建议在虚拟环境中安装。PyPI wheel 已包含自包含应用，不需要安装 .NET runtime 或 SDK；Linux 需要 glibc 2.17 或更高版本。

验证安装：

```bash
crt-cpp-app --version
python -m crt_cpp_app --version
```

## 卸载 Python CLI

在安装该工具的同一个 Python 环境中运行：

```bash
pip uninstall crt-cpp-app
```
