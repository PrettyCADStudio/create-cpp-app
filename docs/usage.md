## 使用方法

### 命令格式

```text
crt-cpp-app [选项]
```

### 命令选项

| 选项 | 说明 |
| --- | --- |
| `-f`、`--force` | 同名项目目录存在时删除并重新创建，不再询问。 |
| `--where` | 输出当前正在运行的可执行程序路径。 |
| `--version` | 显示工具版本。 |
| `-h`、`-?`、`--help` | 显示帮助信息。 |

### 创建项目

在目标父目录中运行：

```bash
crt-cpp-app
```

工具会依次询问项目名称、C++ 标准（17 或 20）以及是否生成 Python 开发辅助脚本。完成后会在当前目录创建项目目录。

项目名称必须以英文字母或下划线开头，后续只能包含英文字母、数字、下划线或连字符。

### 覆盖已有项目

使用 `--force` 跳过确认并覆盖同名目录：

```bash
crt-cpp-app --force
```

该选项会递归删除已有目录，请确认其中内容可以删除。

### 生成的项目

默认结构如下：

```text
my-app/
├── CMakeLists.txt
├── cmake/my-app.cmake
├── inc/my-app.h
├── res/
├── 3rd/
├── patch/
└── src/
    ├── App/
    ├── Static/
    └── Dynamic/
```

进入项目后使用 CMake 构建：

```bash
cd my-app
cmake -S . -B build
cmake --build build --config Release
```

输出位置取决于生成器。Visual Studio 通常为 `bin/Release/App.exe`，Ninja 或 Unix Makefiles 通常为 `bin/App`。

### 使用 Python 开发脚本

创建项目时选择 Python 开发辅助脚本后，会生成以下脚本：

- `mksln.py`：配置 CMake 并生成构建文件。
- `build.py`：配置并编译项目。
- `install.py`：执行 CMake 安装，可通过 `--prefix` 指定安装目录。
- `build-install.py`：依次执行构建和安装。
- `archive.py`：构建项目并将 `bin/` 打包到 `dist/`。

### 直接使用 Python

直接 Python 模式将脚本放在项目根目录，进入项目目录后运行：

```bash
python mksln.py
python build.py --config Release
python install.py --config Release --prefix install
python build-install.py --config Release
python archive.py --config Release
```

### 通过 Pipenv 使用 Python

Pipenv 模式将脚本放在 `scripts/`，并在项目根目录生成 `Pipfile`。首次使用时安装 Pipenv 并创建环境：

```bash
python -m pip install pipenv
pipenv install
```

随后使用 Pipfile 中的快捷命令：

```bash
pipenv run mksln
pipenv run build --config Release
pipenv run install --config Release --prefix install
pipenv run build-install --config Release
pipenv run archive --config Release
```

### 常用查询

```bash
crt-cpp-app --version
crt-cpp-app --where
crt-cpp-app --help
```
