# crt-cpp-app 使用说明

## 简介

`crt-cpp-app` 是一个交互式命令行工具，用于快速创建可由 CMake 构建的 C++ 项目。它会生成一个包含可执行程序、静态库、动态库及常用辅助目录的基础工程。

## 命令格式

```text
crt-cpp-app [选项]
```

## 命令选项

| 选项 | 说明 |
| --- | --- |
| `-f`、`--force` | 如果同名项目目录已存在，删除该目录并重新创建项目，不再请求确认。 |
| `--where` | 输出当前正在运行的 `crt-cpp-app` 可执行程序的完整路径。 |
| `--version` | 显示当前工具版本。 |
| `-h`、`-?`、`--help` | 显示命令帮助和可用选项。 |

查看版本：

```bash
crt-cpp-app --version
```

查看程序所在目录：

```bash
crt-cpp-app --where
```

查看帮助：

```bash
crt-cpp-app --help
```

## 通过 pip 安装

PyPI 提供与操作系统和 CPU 架构匹配的 wheel，其中已包含自包含的 C# 程序，不需要安装 .NET runtime 或 SDK。直接安装即可由 pip 选择当前平台的 wheel：

Linux wheel 使用 `manylinux_2_17` 平台标签，因此要求 glibc 2.17 或更高版本。

```bash
python -m pip install crt-cpp-app
```

安装完成后，确认命令可用：

```bash
crt-cpp-app --version
```

pip 会将命令入口安装到当前 Python 环境的 scripts 目录；使用虚拟环境时，请先激活该虚拟环境。可使用 `python -m crt_cpp_app` 作为等价入口。

## 通过 npm 安装

通过 npm 安装与当前平台对应的包：

```bash
npm install --global crt-cpp-app-win32-x64
```

包名会包含目标平台和架构，例如 Windows x64 为 `crt-cpp-app-win32-x64`。安装后可直接使用 `crt-cpp-app`；npm 包同样内嵌自包含的 C# 程序，因此不需要安装 .NET runtime 或 SDK。

## 通过 NuGet 使用

通过 nuget.org 安装 .NET 全局工具：

```bash
dotnet tool install --global crt-cpp-app
```

安装后执行 `crt-cpp-app`。如果只需要当前目录使用，可以将 `--global` 替换为 `--tool-path <目录>`。

## 发布到 nuget.org

将版本 tag（例如 `v0.1.8`）推送到 GitHub 后，发布工作流会构建代码、打包 NuGet 全局工具并发布到 nuget.org。tag 去掉 `v` 后必须与项目的 `Version` 一致。仓库管理员须先在 GitHub 的 **Settings → Secrets and variables → Actions** 添加名为 `NUGET_API_KEY` 的 repository secret，其值为 nuget.org 的 package push API key。相同版本已存在时会被安全跳过。

工作流也会发布 Python wheel 到 PyPI。请在相同页面添加 `PYPI_API_TOKEN` repository secret，其值为 PyPI 的 API token。首次发布前，`crt-cpp-app` 这一 PyPI 项目名必须已由该 token 对应的账号或组织拥有；已有的 wheel 会自动跳过。

工作流还会发布各平台的 npm 包。请添加 `NPM_TOKEN` repository secret，其值为拥有发布权限的 npm automation token，或已启用 **Bypass 2FA** 的 granular access token。普通 token 会触发 `EOTP`，而 CI 无法提供一次性验证码。当前发布的包名为 `crt-cpp-app-win32-x64`、`crt-cpp-app-linux-x64` 和 `crt-cpp-app-darwin-arm64`；首次发布前，它们必须归该 token 对应的 npm 账号或组织所有。已有版本会自动跳过。

## 创建项目

在希望存放新项目的目录中运行：

```bash
crt-cpp-app
```

工具会依次要求输入以下信息：

1. **项目名称**：会作为新建项目目录和 CMake 项目名称；必须以英文字母或下划线开头，后续只能使用英文字母、数字、下划线或连字符。
2. **C++ 标准**：可选择 C++17 或 C++20，默认是 C++17。
3. **Python 开发辅助脚本**：选择不使用（默认）、直接使用 Python 脚本，或使用 Pipenv 运行脚本。

示例：

```text
Project name: my-app
C++ standard: 20
```

执行完成后，当前目录下会创建 `my-app` 项目目录。

如果系统中可运行 `git`，工具还会初始化 Git 仓库，生成 `.gitignore`，并创建一个包含初始项目文件的提交。`.gitignore` 会忽略构建、安装、归档、IDE 和 Python 缓存文件。选择 Pipenv 模式且系统中可运行 `pipenv` 时，工具会在提交前执行 `pipenv install` 生成 `Pipfile.lock`；未安装 pipenv 或执行失败时会跳过该文件。

## 覆盖已有项目

若当前目录中已有同名目录，工具会询问是否删除该目录后重新创建：

```text
Directory 'my-app' already exists. Delete and recreate?
```

如确认需要覆盖，可输入 `Yes`。

也可以使用 `--force` 或 `-f` 跳过确认：

```bash
crt-cpp-app --force
```

```bash
crt-cpp-app -f
```

`--force` 会递归删除已有的同名目录及其全部内容，请仅在确认内容可被删除时使用。

## 生成的目录结构

项目默认生成以下结构：

```text
my-app/
├── CMakeLists.txt
├── .gitignore
├── cmake/
│   └── my-app.cmake
├── inc/
│   └── my-app.h
├── res/
├── 3rd/
├── patch/
└── src/
    ├── App/
    │   ├── CMakeLists.txt
    │   └── main.cpp
    ├── Static/
    │   ├── CMakeLists.txt
    │   ├── Private/
    │   │   └── StaticLib.cpp
    │   └── Public/
    │       └── StaticLib.h
    └── Dynamic/
        ├── CMakeLists.txt
        ├── Private/
        │   └── DynamicLib.cpp
        └── Public/
            ├── DynamicExports.h
            └── DynamicLib.h
```

`inc/`、`res/`、`3rd/` 和 `patch/` 会始终创建；项目创建完成后，可由用户自行添加内容或删除不需要的目录。

选择直接使用 Python 脚本时，项目根目录还会生成 `mksln.py`、`build.py`、`install.py`、`build-install.py` 和 `archive.py`。选择 Pipenv 时，它们位于 `scripts/`，根目录会额外生成 `Pipfile`。

## 生成内容说明

### App

`src/App` 生成名为 `App` 的可执行目标。其 `main.cpp` 会调用静态库、动态库和 `inc/` 中共享头文件的示例函数。

### Static

`src/Static` 生成名为 `Static` 的静态库。公共头文件位于 `Public/StaticLib.h`，实现文件位于 `Private/StaticLib.cpp`。

### Dynamic

`src/Dynamic` 生成名为 `Dynamic` 的动态库。`Public/DynamicExports.h` 会根据平台生成导出宏：

- Windows 使用 `__declspec(dllexport)` 和 `__declspec(dllimport)`；
- GCC 兼容编译器使用默认符号可见性属性。

### CMake 配置

顶层 `CMakeLists.txt` 会：

- 使用所选的 C++17 或 C++20 标准；
- 添加 `App`、`Static` 和 `Dynamic` 三个子项目；
- 将可执行文件、动态库和静态库的输出目录设置为 `bin/`；
- 为 Debug、Release、MinSizeRel 和 RelWithDebInfo 分别设置输出子目录；
- 引入 `cmake/<项目名>.cmake` 作为项目自定义 CMake 模块的入口。
- 自动搜索并添加 `src/` 下包含 `CMakeLists.txt` 的子项目；新增项目目录后无需修改根 `CMakeLists.txt`。
- 提供 `define_executable`、`define_static_library`、`define_shared_library` 与 `link_internal_projects`，子项目只需声明项目类型和依赖。
- 自动收集可安装的可执行程序、静态库、动态库和模块库；调用 `cmake --install` 时，运行时产物安装到 `bin/`，库安装到 `lib/`。

## 构建生成的项目

进入生成的项目目录并运行：

```bash
cd my-app
cmake -S . -B build
cmake --build build --config Release
```

第一条命令生成构建系统，第二条命令编译项目。

在多配置生成器（如 Visual Studio）中，Release 可执行文件通常位于：

```text
bin/Release/App.exe
```

在单配置生成器（如 Ninja 或 Unix Makefiles）中，可执行文件通常位于：

```text
bin/App
```

生成路径会随所使用的 CMake 生成器和构建配置变化。

## 使用生成的 Python 开发脚本

脚本模式需要 Python 3 和 CMake。直接 Python 模式在项目根目录运行：

```bash
python mksln.py
python build.py --config Release
python install.py --config Release --prefix install
python build-install.py --config Release
python archive.py --config Release
```

`install.py` 默认将内容安装到 `<项目目录>/install`；可通过 `--prefix` 覆盖。`archive.py` 会先编译项目，再将 `bin/` 打包到 `dist/`。

Pipenv 模式使用根目录 `Pipfile` 定义的快捷命令：

```bash
pipenv run mksln
pipenv run build --config Release
pipenv run install --config Release --prefix install
pipenv run build-install --config Release
pipenv run archive --config Release
```

## 自定义项目

创建完成后可按需要修改：

- `src/App/main.cpp` 中的应用入口；
- `src/Static` 中的静态库接口和实现；
- `src/Dynamic` 中的动态库接口和实现；
- `cmake/<项目名>.cmake` 中的项目级 CMake 函数和配置；
- 顶层 `CMakeLists.txt` 中的构建选项、依赖和子项目配置。
