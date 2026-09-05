# coding: utf-8

import subprocess
import platform
from pathlib import Path
from typing import Final, Sequence

APPLICATION_NAME: Final[str] = "crt-cpp-app"

def get_repo_dir() -> Path:
    return Path(__file__).resolve().parent.parent

def get_scripts_dir() -> Path:
    return get_repo_dir() / "scripts"

def get_src_dir() -> Path:
    return get_repo_dir() / "src"

def get_test_dir() -> Path:
    return get_repo_dir() / "test"

def get_docs_dir() -> Path:
    return get_repo_dir() / "docs"

def get_bin_dir() -> Path:
    return get_repo_dir() / "bin"

def get_test_bin_dir() -> Path:
    return get_bin_dir() / "test"

def get_dist_dir() -> Path:
    return get_repo_dir() / "dist"

def get_build_dir() -> Path:
    return get_repo_dir() / "build"

def get_python_dir() -> Path:
    return get_repo_dir() / "python"

def get_nodejs_dir() -> Path:
    return get_repo_dir() / "nodejs"

def get_sln_file() -> Path:
    return get_repo_dir() / f"{APPLICATION_NAME}.slnx"

def get_proj_dir() -> Path:
    return get_src_dir() / APPLICATION_NAME

def get_proj_file() -> Path:
    return get_proj_dir() / f"{APPLICATION_NAME}.csproj"

def get_test_proj_dir() -> Path:
    return get_test_dir() / f"{APPLICATION_NAME}.Tests"

def get_test_proj_file() -> Path:
    return get_test_proj_dir() / f"{APPLICATION_NAME}.Tests.csproj"

def get_install_file() -> Path:
    return get_src_dir() / "install.py"

def run_command(command_args: Sequence[str], *args, **kwargs):
    print(f"Will run command: {' '.join(command_args)}")
    return subprocess.run(command_args, *args, **kwargs)

def path_to_str(path: Path) -> str:
    return str(path)

def get_platform_name() -> str:
    system_names = {
        "Darwin": "macos",
        "Linux": "linux",
        "Windows": "windows",
    }
    system = platform.system()
    return system_names.get(system, system.lower())

def get_architecture_name() -> str:
    architecture_names = {
        "AMD64": "x64",
        "arm64": "arm64",
        "aarch64": "arm64",
        "i386": "x86",
        "i686": "x86",
        "x86_64": "x64",
    }
    architecture = platform.machine()
    return architecture_names.get(architecture, architecture.lower())

def get_runtime_identifier():
    system_names = {
        "Darwin": "osx",
        "Linux": "linux",
        "Windows": "win",
    }
    platform_name = system_names.get(platform.system())
    if not platform_name:
        raise RuntimeError(f"Unsupported platform: {platform.system()}")
    return f"{platform_name}-{get_architecture_name()}"
