# create-cpp-app

一个用于快速创建 C++ CMake 项目的命令行脚手架工具。通过交互式问答引导你完成项目初始化，自动生成标准化的目录结构和 CMake 配置。

## 功能

- 交互式输入项目名称（支持非空校验）
- 选择 C++ 标准版本（C++17 / C++20，默认 C++17）
- 自动生成 CMake 项目结构，包含解决方案和可执行目标
- 输出目录统一设置为项目下的 `bin/` 文件夹

## 生成的项目结构

```
<项目名>/
├── CMakeLists.txt          # 解决方案级 CMake 配置
└── src/
    └── App/
        ├── CMakeLists.txt  # App 可执行目标定义
        └── main.cpp        # 入口源文件
```

## 环境要求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/) 或更高版本
- Python 3（用于构建和打包脚本）

## 快速开始

### 从源码构建

```bash
# 编译（默认 Release）
python build.py

# 编译 Debug 版本
python build.py --config Debug

# 运行
dotnet run --project src/create-cpp-app/create-cpp-app.csproj
```

### 打包与安装

```bash
# 打包为 zip（输出到 dist/ 目录）
python archive.py

# 解压后运行安装脚本，将 create-cpp-app 安装到 ~/.create-cpp-app 并配置 PATH
python install.py
```

### 清理构建产物

```bash
# 清理编译中间文件和输出
python clean.py

# 同时清理 dist 目录
python clean.py --dist
```

## 使用示例

运行 `create-cpp-app` 后按提示操作：

```
? Project name: my-awesome-app
? C++ standard: C++17

Project 'my-awesome-app' created at ./my-awesome-app
  C++ standard: C++17
  Output directory: bin/
```

生成后即可使用 CMake 构建：

```bash
cd my-awesome-app
cmake -B build
cmake --build build
./bin/App
```

## 项目结构（本工具自身）

```
create-cpp-app/
├── build.py                        # 编译脚本
├── archive.py                      # 打包脚本
├── clean.py                        # 清理脚本
├── create-cpp-app.slnx             # .NET 解决方案文件
└── src/
    ├── create-cpp-app/             # 主程序
    │   ├── create-cpp-app.csproj
    │   └── Program.cs
    └── install.py                  # 安装脚本（打包到 zip 中）
```

## 许可证

[MIT](LICENSE) &copy; PrettyCAD Studio
