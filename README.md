# crt-cpp-app

`crt-cpp-app` 是一个跨平台的 C++ CMake 项目脚手架命令行工具。它通过交互式配置快速创建包含可执行程序、静态库、动态库和工程级 CMake 配置的项目骨架，支持 C++17 和 C++20。

## 发行产物

所有发行方式提供相同的 `crt-cpp-app` 命令和参数，区别仅在于安装方式：

- **本地应用**：下载对应平台的自包含 ZIP，解压后即可运行，不需要 .NET runtime。
- **NuGet CLI**：作为 .NET 全局工具安装，适合 .NET 和跨平台开发环境。
- **Python CLI**：通过 PyPI 和 pip 安装，wheel 内含对应平台的本地应用。
- **Node.js CLI**：通过 npm 安装与平台和架构匹配的包，适合 Node.js 工具链。

安装时的系统要求和卸载方法见：

- [本地应用安装](docs/install-app.md)
- [Python CLI 安装](docs/install-python-cli.md)
- [NuGet CLI 安装](docs/install-nuget-cli.md)
- [Node.js CLI 安装](docs/install-nodejs-cli.md)

## 使用方法

安装任一发行产物后，使用方式完全相同：

```bash
crt-cpp-app
crt-cpp-app --help
crt-cpp-app --version
```

完整的命令选项、项目结构、CMake 构建和 Python 开发脚本说明见 [使用方法](docs/usage.md)。

## 开发环境

构建和测试本项目需要：

- Python 3.12（用于仓库脚本和 Pipenv 环境）
- Pipenv
- .NET 10 SDK
- CMake 3.20 或更高版本
- C++ 编译器（用于测试生成的项目）
- Node.js 和 npm（仅在构建 Node.js 产物时需要）

构建 Python wheel 时，项目会使用 `python/pyproject.toml` 和 `python/setup.py`，并将自包含的 .NET 应用嵌入 wheel。NuGet、Python 和 Node.js 产物均由仓库脚本生成。

## 开发命令

```bash
pipenv install --dev --deploy
pipenv run build
pipenv run test
pipenv run archive --all
```

产物输出到 `dist/`。单独生成某种产物时可使用 `pipenv run archive --zip`、`--python`、`--nuget` 或 `--nodejs`。

## 文档

- [文档标题与简介](docs/title.md)
- [使用方法](docs/usage.md)
- [本地应用安装与卸载](docs/install-app.md)
- [Python CLI 安装与卸载](docs/install-python-cli.md)
- [NuGet CLI 安装与卸载](docs/install-nuget-cli.md)
- [Node.js CLI 安装与卸载](docs/install-nodejs-cli.md)
- [许可信息](docs/copyright.md)

## 许可

本项目采用 [MIT License](docs/copyright.md) 发布。
