# crt-cpp-app

crt-cpp-app 是一个用于快速生成 C++ CMake 项目的 CLI 脚手架工具。它会根据用户输入生成可直接构建的项目骨架，并支持静态库、动态库和可执行程序的标准目录结构。

## 功能概览

- 交互式创建新项目
- 选择 C++ 标准：17 / 20
- 默认生成 `inc/`、`res/`、`3rd/` 和 `patch/` 常用目录
- 自动生成以下结构：
  - `src/App` 可执行程序
  - `src/Static` 静态库
  - `src/Dynamic` 动态库
  - 顶层 `CMakeLists.txt`
- 工程级 `cmake/` 目录
- 自动发现 `src/` 下的 CMake 子项目，并提供统一的可执行程序、静态库和动态库定义函数
- 可选生成 Python 开发脚本，支持直接运行或通过 Pipenv 命令运行
- 检测到 Git 时自动初始化仓库、生成 `.gitignore` 并创建初始提交
- 统一输出目录为 `bin/`
- 提供 build / archive / install / clean / test 脚本

## 环境要求

- Python 3
- CMake 3.20+
- 可选：Windows / Linux / macOS

从源码构建 wheel 时还需要 .NET 10 SDK 或更高版本；通过 pip 安装已构建的 wheel 时不需要 .NET runtime 或 SDK。

Linux wheel 采用 `manylinux_2_17` 标签，要求系统使用 glibc 2.17 或更高版本。

## 安装与运行

### 1) 通过 pip 安装

从 PyPI 安装与当前操作系统和 CPU 架构匹配的 wheel：

```bash
python -m pip install crt-cpp-app
```

安装后可直接运行：

```bash
crt-cpp-app
```

也可以使用模块入口：

```bash
python -m crt_cpp_app
```

### 2) 从源码构建 wheel

```bash
python -m pip install build
python -m build --wheel python
```

构建过程会发布一个包含自包含 C# 程序的、与当前平台绑定的 wheel 到 `dist/`。

### 3) 开发时运行 CLI

```bash
dotnet run --project src/crt-cpp-app/crt-cpp-app.csproj
```

或直接使用安装后的命令（如果已执行安装脚本）：

```bash
crt-cpp-app
```

## 版本信息

```bash
crt-cpp-app --version
```

输出：

```text
0.1.8
```

## 查看程序所在目录

```bash
crt-cpp-app --where
```

该命令输出当前正在运行的 `crt-cpp-app` 可执行程序的完整路径。

## 交互式创建示例

运行后会询问：

```text
Project name: my-awesome-app
C++ standard: 17
```

生成目录如下：

```text
my-awesome-app/
├── CMakeLists.txt
├── .gitignore
├── cmake/
│   └── my-awesome-app.cmake
├── inc/
├── res/
├── 3rd/
├── patch/
├── src/
│   ├── App/
│   │   ├── CMakeLists.txt
│   │   └── main.cpp
│   ├── Dynamic/
│   │   ├── CMakeLists.txt
│   │   ├── Private/
│   │   │   └── DynamicLib.cpp
│   │   └── Public/
│   │       ├── DynamicExports.h
│   │       └── DynamicLib.h
│   └── Static/
│       ├── CMakeLists.txt
│       ├── Private/
│       │   └── StaticLib.cpp
│       └── Public/
│           └── StaticLib.h
└── bin/                  # 产物输出目录
```

选择 Python 脚本开发辅助时，还会生成 `mksln.py`、`build.py`、`install.py`、`build-install.py` 和 `archive.py`。直接运行模式将它们放在项目根目录；Pipenv 模式将它们放在 `scripts/`，并生成根目录 `Pipfile`。

如果系统可以运行 `git`，项目创建完成后会自动执行 Git 初始化并创建初始提交。生成的 `.gitignore` 会排除构建、安装、归档、IDE 和 Python 缓存文件。选择 Pipenv 脚本模式且系统可运行 `pipenv` 时，会先执行 `pipenv install` 生成 `Pipfile.lock`，再将其包含在初始提交中。

## 生成的 CMake 项目构建方式

进入生成目录后：

```bash
cd my-awesome-app
cmake -S . -B build
cmake --build build --config Release
```

根 CMake 文件会自动搜索 `src/` 下所有包含 `CMakeLists.txt` 的子目录，并安装其中可安装的可执行程序和库目标。新增符合该约定的项目后，无需修改根 CMake 文件。

## 生成项目的 Python 开发脚本

创建项目时可选择不生成脚本（默认）、直接使用 Python，或使用 Pipenv。脚本需要 Python 3 与 CMake。

直接模式在项目根目录执行：

```bash
python mksln.py
python build.py --config Release
python install.py --config Release --prefix install
python build-install.py --config Release
python archive.py --config Release
```

Pipenv 模式使用等价命令：

```bash
pipenv run mksln
pipenv run build --config Release
pipenv run install --config Release --prefix install
pipenv run build-install --config Release
pipenv run archive --config Release
```

`mksln.py` 配置 CMake 并生成构建文件；`build.py` 配置并编译；`install.py` 执行 `cmake --install`；`build-install.py` 合并编译和安装；`archive.py` 将构建后的 `bin/` 打包至 `dist/`。

运行程序：

```bash
./bin/Release/App
```

在 Windows 下，通常输出到：

```text
bin\Release\App.exe
```

## 现成脚本

仓库脚本使用 Pipenv 管理环境。首次使用前请安装 Pipenv，并在仓库根目录执行 `pipenv install --dev --deploy`。

### 构建 C# 开发版本

```bash
pipenv install --dev --deploy
pipenv run build
```

### 读取或更新版本

```bash
pipenv run version
pipenv run version --update 0.1.9
```

无参数时输出 `crt-cpp-app.csproj` 的 `Version`。`--update` 仅接受三段或四段数字版本号，并会同步更新 `Version`、`AssemblyVersion`、`FileVersion` 和 `InformationalVersion`。

### 测试脚本

```bash
pipenv run test
```

执行后，会自动运行当前仓库中的单元测试（基于 `dotnet test`）。测试在系统临时目录中创建独占的随机目录，并在完成后清理；它不会删除仓库内的用户文件。

### 打包脚本

```bash
pipenv run archive --zip
```

默认输出为文件夹归档。`--zip` 生成自包含程序的 ZIP 发布包，`--python` 生成可通过 pip 安装的 platform wheel，`--nuget` 生成 NuGet 包，`--nodejs` 生成 npm 包；`--all` 同时生成四者。所有产物均写入 `dist/`。

`--nuget` 生成可供 .NET 项目使用的 `.nupkg` 包。`--nodejs` 生成可通过 npm 安装的、与当前平台绑定的 `.tgz` 包；安装后同样可直接调用 `crt-cpp-app`。`--all` 会同时生成 ZIP、Python wheel、NuGet 和 Node.js 包。

### 发布 NuGet 包

推送形如 `v0.1.8` 的版本 tag 会触发 GitHub Actions：它会在 Windows、Linux 和 macOS 上构建发布包，并在 Linux 上重新打包一次 NuGet 全局工具后发布到 nuget.org。tag 去掉 `v` 后必须与项目的 `Version` 一致。发布前，请在仓库的 **Settings → Secrets and variables → Actions** 中创建 `NUGET_API_KEY` secret，值为 nuget.org 为该包创建的 package push API key。工作流使用 `--skip-duplicate`，因此已发布的相同版本不会导致重试失败。

同一个工作流也会将三个平台的 Python wheel 发布到 PyPI。请在同一位置创建 `PYPI_API_TOKEN` secret，值为 PyPI 创建的 API token；首次发布前还应确认 PyPI 上的 `crt-cpp-app` 项目名归当前账号或组织所有。已存在的 wheel 会被跳过。

同一个工作流还会将 Windows、Linux 和 macOS 对应的 npm 包发布到 npmjs.com。请创建 `NPM_TOKEN` secret，值为对 `@pcads` scope 具备发布权限的 npm automation token，或已启用 **Bypass 2FA** 的 granular access token；普通 token 会因 npm 要求一次性验证码（`EOTP`）而无法在 CI 中发布。当前三个包名分别为 `@pcads/crt-cpp-app-win32-x64`、`@pcads/crt-cpp-app-linux-x64` 和 `@pcads/crt-cpp-app-darwin-arm64`；重跑工作流时，已存在的包版本会被跳过。

### 安装脚本

```bash
python src/install.py --source dist/<archive-folder>
```

此脚本会先在临时目录验证并复制发布包，成功后再替换用户目录下的 `~/.crt-cpp-app`，并尝试更新系统 PATH。

### 清理脚本

```bash
pipenv run clean
```

清理工具与测试项目的 `obj/` 目录、共享的 `bin/` 构建产物，以及系统临时目录中以 `crt-cpp-app-` 开头的残留目录（包括测试和发布过程的临时文件）。如果需要额外清理 `dist`：

```bash
pipenv run clean --include-dist-dir
```

## 本项目自身结构

```text
crt-cpp-app/
├── scripts/
│   ├── build.py                 # 构建 C# 开发版本
│   ├── test.py                  # 运行单元测试
│   ├── archive.py               # 创建发布归档
│   ├── clean.py                 # 清理中间产物
│   └── version.py               # 读取或更新版本
├── python/
│   ├── pyproject.toml            # Python 包构建配置
│   ├── setup.py                  # 构建时发布并内嵌 C# CLI
│   └── crt_cpp_app/              # pip 安装后的 Python 启动器
├── crt-cpp-app.slnx             # .NET 解决方案文件
├── src/
│   ├── crt-cpp-app/
│   │   ├── Program.cs
│   │   ├── UserInteraction.cs
│   │   ├── CMakeProjectCreator.cs
│   │   ├── CMakeProjectSettings.cs
│   │   └── crt-cpp-app.csproj
│   └── install.py               # 安装脚本
├── test/
│   ├── crt-cpp-app.Tests/
│   └── fixtures/
├── LICENSE
├── README.md
└── .gitignore
```

## 开发与测试

```bash
pipenv run test
```

测试逻辑会：

1. 根据 fixture 配置生成示例项目
2. 检查文件树和文件内容是否匹配 fixture
3. 真实运行 `cmake -S ... -B ...`
4. 真实运行 `cmake --build ...`
5. 仅当所有流程都成功时才视为测试通过

## 许可证

MIT License

版权：PrettyCAD Studio
