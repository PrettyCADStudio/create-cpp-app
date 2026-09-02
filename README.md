# create-cpp-app

create-cpp-app 是一个用于快速生成 C++ CMake 项目的 CLI 脚手架工具。它会根据用户输入生成可直接构建的项目骨架，并支持静态库、动态库和可执行程序的标准目录结构。

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

## 安装与运行

### 1) 通过 pip 安装

安装与当前操作系统和 CPU 架构匹配的 wheel：

```bash
python -m pip install create_cpp_app-<version>-py3-none-<platform>.whl
```

安装后可直接运行：

```bash
create-cpp-app
```

也可以使用模块入口：

```bash
python -m create_cpp_app
```

### 2) 从源码构建 wheel

```bash
python -m pip install build
python -m build --wheel
```

构建过程会发布一个包含自包含 C# 程序的、与当前平台绑定的 wheel 到 `dist/`。

### 3) 开发时运行 CLI

```bash
dotnet run --project src/create-cpp-app/create-cpp-app.csproj
```

或直接使用安装后的命令（如果已执行安装脚本）：

```bash
create-cpp-app
```

## 版本信息

```bash
create-cpp-app --version
```

输出：

```text
0.1.1
```

## 查看程序所在目录

```bash
create-cpp-app --where
```

该命令输出当前正在运行的 `create-cpp-app` 可执行程序的完整路径。

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

### 构建 C# 开发版本

```bash
python build-dotnet.py
```

### 测试脚本

```bash
python test.py
```

执行后，会自动运行当前仓库中的单元测试（基于 `dotnet test`）。测试在系统临时目录中创建独占的随机目录，并在完成后清理；它不会删除仓库内的用户文件。

### 打包脚本

```bash
python archive.py --zip
```

默认输出为文件夹归档。`--zip` 生成自包含程序的 ZIP 发布包，`--python` 生成可通过 pip 安装的 platform wheel，`--all` 同时生成两者；所有产物均写入 `dist/`。

`--nodejs` 生成可通过 npm 安装的、与当前平台绑定的 `.tgz` 包；安装后同样可直接调用 `create-cpp-app`。`--all` 会同时生成 ZIP、Python wheel 和 Node.js 包。

### 安装脚本

```bash
python src/install.py --source dist/<archive-folder>
```

此脚本会先在临时目录验证并复制发布包，成功后再替换用户目录下的 `~/.create-cpp-app`，并尝试更新系统 PATH。

### 清理脚本

```bash
python clean.py
```

清理工具与测试项目的 `obj/` 目录、共享的 `bin/` 构建产物，以及系统临时目录中以 `create-cpp-app-` 开头的残留目录（包括测试和发布过程的临时文件）。如果需要额外清理 `dist`：

```bash
python clean.py --dist
```

## 本项目自身结构

```text
create-cpp-app/
├── build-dotnet.py               # 构建 C# 开发版本
├── test.py                      # 运行单元测试
├── archive.py                   # 创建发布归档
├── clean.py                     # 清理中间产物
├── pyproject.toml                # Python 包构建配置
├── setup.py                      # 构建时发布并内嵌 C# CLI
├── python/create_cpp_app/        # pip 安装后的 Python 启动器
├── create-cpp-app.slnx          # .NET 解决方案文件
├── src/
│   ├── create-cpp-app/
│   │   ├── Program.cs
│   │   ├── UserInteraction.cs
│   │   ├── CMakeProjectCreator.cs
│   │   ├── CMakeProjectSettings.cs
│   │   └── create-cpp-app.csproj
│   └── install.py               # 安装脚本
├── test/
│   ├── create-cpp-app.Tests/
│   └── fixtures/
├── LICENSE
├── README.md
└── .gitignore
```

## 开发与测试

```bash
python test.py
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
