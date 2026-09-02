from __future__ import annotations

import platform
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

from setuptools import Distribution, find_packages, setup
from setuptools.command.bdist_wheel import bdist_wheel as _bdist_wheel
from setuptools.command.build_py import build_py as _build_py


ROOT = Path(__file__).parent.resolve()
PROJECT_FILE = ROOT / "src" / "crt-cpp-app" / "crt-cpp-app.csproj"
PACKAGE_NAME = "crt_cpp_app"


def read_version() -> str:
    root = ET.parse(PROJECT_FILE).getroot()
    version = root.findtext(".//Version")
    if not version:
        raise RuntimeError(f"Version is missing from {PROJECT_FILE}")
    return version


def runtime_identifier() -> str:
    systems = {"Windows": "win", "Linux": "linux", "Darwin": "osx"}
    architectures = {
        "AMD64": "x64", "x86_64": "x64", "i386": "x86", "i686": "x86",
        "arm64": "arm64", "aarch64": "arm64",
    }
    system = systems.get(platform.system())
    architecture = architectures.get(platform.machine())
    if not system or not architecture:
        raise RuntimeError(
            f"Unsupported platform for the bundled application: "
            f"{platform.system()} {platform.machine()}"
        )
    return f"{system}-{architecture}"


class BuildPy(_build_py):
    """Publish the .NET command into the Python package build directory."""

    def run(self) -> None:
        super().run()
        output_dir = Path(self.build_lib) / PACKAGE_NAME / "_app"
        output_dir.mkdir(parents=True, exist_ok=True)
        subprocess.run(
            [
                "dotnet", "publish", str(PROJECT_FILE), "--configuration", "Release",
                "--runtime", runtime_identifier(), "--self-contained", "true",
                "--output", str(output_dir),
            ],
            check=True,
        )


class BDistWheel(_bdist_wheel):
    """The Python wrapper is portable, but its bundled application is not."""

    def finalize_options(self) -> None:
        super().finalize_options()
        self.root_is_pure = False

    def get_tag(self):
        _, _, platform_tag = super().get_tag()
        return "py3", "none", platform_tag


class BinaryDistribution(Distribution):
    """Mark the wheel as platform-specific because it embeds a native app."""

    def has_ext_modules(self) -> bool:
        return True


setup(
    name="crt-cpp-app",
    version=read_version(),
    description="Create a C++ CMake project from the command line",
    long_description=(ROOT / "README.md").read_text(encoding="utf-8"),
    long_description_content_type="text/markdown",
    license="MIT",
    python_requires=">=3.9",
    packages=find_packages("python"),
    package_dir={"": "python"},
    entry_points={"console_scripts": ["crt-cpp-app=crt_cpp_app.cli:main"]},
    cmdclass={"build_py": BuildPy, "bdist_wheel": BDistWheel},
    distclass=BinaryDistribution,
)
