# create-cpp-app 使用说明

## 简介

`create-cpp-app` 是一个交互式命令行工具，用于快速创建可由 CMake 构建的 C++ 项目。它会生成一个包含可执行程序、静态库和动态库的基础工程，并可按需创建常用辅助目录。

## 命令格式

```text
create-cpp-app [选项]
```

## 命令选项

| 选项 | 说明 |
| --- | --- |
| `-f`、`--force` | 如果同名项目目录已存在，删除该目录并重新创建项目，不再请求确认。 |
| `--version` | 显示当前工具版本。 |
| `-h`、`-?`、`--help` | 显示命令帮助和可用选项。 |

查看版本：

```bash
create-cpp-app --version
```

查看帮助：

```bash
create-cpp-app --help
```

## 从发布包安装

GitHub Release 中的 ZIP 包是针对对应操作系统和架构生成的自包含发布包，包含可执行程序、运行所需文件、`doc.md` 和 `install.py`，无需预先安装 .NET runtime。下载与解压适用于当前操作系统和架构的归档文件后，在解压目录中执行安装脚本：

```bash
python install.py
```

例如，Windows x64 用户应下载名称类似以下的发布资产：

```text
create-cpp-app-v0.0.1-windows-x64.zip
```

### 安装时的变更

`install.py` 会执行以下操作：

1. 验证发布包，并先将程序文件复制到同一磁盘的临时目录；
2. 成功后将已有的 `~/.create-cpp-app` 备份并替换为新版本；
3. 将该安装目录添加到当前用户的 `PATH`。

因此，重新运行安装脚本会覆盖旧版本。请勿将其他个人文件保存在 `~/.create-cpp-app` 中。

在 Windows 上，脚本修改用户级 `PATH`，需要关闭并重新打开终端后才会生效。在 Linux 和 macOS 上，脚本会根据当前 shell 将 PATH 配置追加到相应的 shell 配置文件；重新打开终端，或重新加载该配置文件后即可使用。

安装完成且终端已重新打开后，确认命令可用：

```bash
create-cpp-app --version
```

如果安装脚本无法识别当前 shell，它会提示手动将 `~/.create-cpp-app` 添加至 PATH。

## 创建项目

在希望存放新项目的目录中运行：

```bash
create-cpp-app
```

工具会依次要求输入以下信息：

1. **项目名称**：不能为空，且会作为新建项目目录和 CMake 项目名称。
2. **C++ 标准**：可选择 C++17 或 C++20，默认是 C++17。
3. **共享头文件目录**：是否创建 `inc/`。
4. **资源目录**：是否创建 `res/`。
5. **第三方库目录**：是否创建 `3rd/`。
6. **补丁目录**：是否创建 `patch/`。

示例：

```text
Project name: my-app
C++ standard: 20
Create 'inc' folder for shared headers?: Yes
Create 'res' folder for resource files?: No
Create '3rd' folder for third-party library files in the project?: Yes
Create 'patch' folder for patches?: No
```

执行完成后，当前目录下会创建 `my-app` 项目目录。

## 覆盖已有项目

若当前目录中已有同名目录，工具会询问是否删除该目录后重新创建：

```text
Directory 'my-app' already exists. Delete and recreate?
```

如确认需要覆盖，可输入 `Yes`。

也可以使用 `--force` 或 `-f` 跳过确认：

```bash
create-cpp-app --force
```

```bash
create-cpp-app -f
```

`--force` 会递归删除已有的同名目录及其全部内容，请仅在确认内容可被删除时使用。

## 生成的目录结构

启用所有可选目录时，生成的项目结构如下：

```text
my-app/
├── CMakeLists.txt
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

未选择的 `inc/`、`res/`、`3rd/` 和 `patch/` 目录不会生成。

## 生成内容说明

### App

`src/App` 生成名为 `App` 的可执行目标。其 `main.cpp` 会调用静态库和动态库的示例函数；启用 `inc/` 时，也会包含并调用共享头文件中的示例函数。

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

## 自定义项目

创建完成后可按需要修改：

- `src/App/main.cpp` 中的应用入口；
- `src/Static` 中的静态库接口和实现；
- `src/Dynamic` 中的动态库接口和实现；
- `cmake/<项目名>.cmake` 中的项目级 CMake 函数和配置；
- 顶层 `CMakeLists.txt` 中的构建选项、依赖和子项目配置。
