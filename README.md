# create-cpp-app

create-cpp-app 是一个用于快速生成 C++ CMake 项目的 CLI 脚手架工具。它会根据用户输入生成可直接构建的项目骨架，并支持静态库、动态库和可执行程序的标准目录结构。

## 功能概览

- 交互式创建新项目
- 选择 C++ 标准：17 / 20
- 可选生成 `inc/` 共享头文件目录
- 自动生成以下结构：
  - `src/App` 可执行程序
  - `src/Static` 静态库
  - `src/Dynamic` 动态库
  - 顶层 `CMakeLists.txt`
  - 工程级 `cmake/` 目录
- 统一输出目录为 `bin/`
- 提供 build / archive / install / clean / test 脚本

## 环境要求

- .NET 10 SDK 或更高版本
- Python 3
- CMake 3.20+
- 可选：Windows / Linux / macOS

## 安装与运行

### 1) 构建当前工具本身

```bash
python build.py
```

默认构建 Release 版本，也可以指定 Debug：

```bash
python build.py --config Debug
```

### 2) 运行 CLI

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
0.0.1
```

## 交互式创建示例

运行后会询问：

```text
Project name: my-awesome-app
C++ standard: 17
Add 'inc' folder for shared headers? (y/N)
```

生成目录如下：

```text
my-awesome-app/
├── CMakeLists.txt
├── cmake/
├── inc/                  # 可选
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

## 生成的 CMake 项目构建方式

进入生成目录后：

```bash
cd my-awesome-app
cmake -S . -B build
cmake --build build --config Release
```

运行程序：

```bash
./bin/Release/App
```

在 Windows 下，通常输出到：

```text
bin\Release\App.exe
```

## 现成脚本

### 构建脚本

```bash
python build.py
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

默认输出为文件夹归档；如果传入 `--zip`，则生成 zip 包到 `dist/`。

### 安装脚本

```bash
python src/install.py --source dist/<archive-folder>
```

此脚本会先在临时目录验证并复制发布包，成功后再替换用户目录下的 `~/.create-cpp-app`，并尝试更新系统 PATH。

### 清理脚本

```bash
python clean.py
```

清理 build、obj、bin 及其他中间产物；如果需要额外清理 `dist`：

```bash
python clean.py --dist
```

## 本项目自身结构

```text
create-cpp-app/
├── build.py                     # 构建本工具
├── test.py                      # 运行单元测试
├── archive.py                   # 创建发布归档
├── clean.py                     # 清理中间产物
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
