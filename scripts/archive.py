# coding: utf-8

import glob
import json
import os
import platform
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path
from typing import Final, Tuple, Sequence

from shared import (
    APPLICATION_NAME, get_platform_name, get_architecture_name, get_runtime_identifier, path_to_str, run_command,
    get_proj_file, get_dist_dir, get_repo_dir, get_install_file, get_docs_dir
)
from version import read_version_info


NPM_PACKAGE_SCOPE: Final[str] = "@pcads"
DOCUMENTATION_FILE: Final[Path] = get_docs_dir() / "doc.md"
NODE_LAUNCHER: Final[Path] = get_repo_dir() / "nodejs" / "crt-cpp-app.js"


def publish_application(output_dir: Path):
    proj_file = get_proj_file()
    args = [
        "dotnet",
        "publish",
        path_to_str(proj_file),
        "-c", "Release",
        "-r", get_runtime_identifier(),
        "--self-contained", "true",
        "--output", path_to_str(output_dir),
    ]
    result = run_command(args, stdout=sys.stderr)
    if result.returncode != 0:
        raise RuntimeError("Failed to publish a self-contained release package")


def collect_files(build_dir: Path) -> Sequence[Tuple[Path, Path]]:
    files = []
    for root, _, filenames in os.walk(build_dir):
        root_dir = Path(root)
        for filename in filenames:
            if filename.endswith(".pdb"):
                continue
            file_path = root_dir / filename
            rel_path = file_path.relative_to(build_dir)
            files.append((file_path, rel_path))

    install_file = get_install_file()
    if install_file.is_file():
        files.append((install_file, install_file.name))

    files.append((DOCUMENTATION_FILE, DOCUMENTATION_FILE.name))
    return files


def get_zip_filename(version: str) -> str:
    platform_name = get_platform_name()
    architecture_name = get_architecture_name()
    return f"{APPLICATION_NAME}-v{version}-{platform_name}-{architecture_name}"


def archive_zip(version: str, dist_dir: Path) -> Path:
    prefix = f"{APPLICATION_NAME}-publish-"
    with tempfile.TemporaryDirectory(prefix=prefix) as publish_dir_str:
        publish_dir = Path(publish_dir_str)
        publish_application(publish_dir)
        files = collect_files(publish_dir)

        name = get_zip_filename(version)
        zip_file = dist_dir / f"{name}.zip"
        with tempfile.NamedTemporaryFile(prefix=f".{name}-", suffix=".zip", dir=dist_dir, delete=False) as tmp:
            temp_zip_path = tmp.name
        with zipfile.ZipFile(temp_zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
            for file_path, file_rel_path in files:
                zf.write(file_path, file_rel_path)
        os.replace(temp_zip_path, zip_file)
        return zip_file


def archive_folder(version: str, dist_dir: Path) -> Path:
    prefix = f"{APPLICATION_NAME}-publish-"
    with tempfile.TemporaryDirectory(prefix=prefix) as publish_dir_str:
        publish_dir = Path(publish_dir_str)
        publish_application(publish_dir)
        files = collect_files(publish_dir)

        name = get_zip_filename(version)
        folder_path = dist_dir / name
        temp_dir_str = tempfile.mkdtemp(prefix=f".{name}-", dir=dist_dir)
        temp_dir = Path(temp_dir_str)
        for file_path, file_rel_path in files:
            dest = temp_dir / file_rel_path
            dest.parent.mkdir(exist_ok=True)
            shutil.copy2(file_path, dest)
        if os.path.exists(folder_path):
            shutil.rmtree(folder_path)
        os.replace(temp_dir_str, folder_path)
        return folder_path


def get_python_wheel_filename(version: str, suffix: str) -> str:
    return f"{APPLICATION_NAME.replace('-', '_')}-{version}-{suffix}.whl"


def archive_python(version, dist_dir: Path) -> Path:
    python_project_dir = get_repo_dir() / "python"
    args = [
        sys.executable, "-m", "pip", "wheel", path_to_str(python_project_dir),
        "--wheel-dir", path_to_str(dist_dir),
        "--no-deps"
    ]
    result = run_command(args)
    if result.returncode != 0:
        raise RuntimeError("Failed to build the Python wheel")

    python_wheel_filename = get_python_wheel_filename(version, '*')
    python_wheel_path = dist_dir / python_wheel_filename
    wheels = glob.glob(path_to_str(python_wheel_path))
    if not wheels:
        raise RuntimeError("Python wheel build completed but no wheel was found")

    wheel_path = max(wheels, key=os.path.getmtime)
    return Path(wheel_path)


def archive_nuget(version: str, dist_dir: Path) -> Path:
    proj_file = get_proj_file()
    args = ["dotnet", "pack", path_to_str(proj_file), "-c", "Release", "--output", path_to_str(dist_dir)]
    result = run_command(args)
    if result.returncode != 0:
        raise RuntimeError("Failed to build the NuGet package")

    nuget_filename = f"{APPLICATION_NAME}.{version}.nupkg"
    nuget_path = dist_dir / nuget_filename
    packages = glob.glob(path_to_str(nuget_path))
    if not packages:
        raise RuntimeError("NuGet pack completed but no package was found")

    package_path = max(packages, key=os.path.getmtime)
    return Path(package_path)


def get_node_platform_name():
    node_platforms = {
        "Darwin": "darwin",
        "Linux": "linux",
        "Windows": "win32",
    }
    node_platform = node_platforms.get(platform.system())
    if not node_platform:
        raise RuntimeError(f"Unsupported Node.js platform: {platform.system()}")
    return node_platform


def archive_nodejs(version, dist_dir: Path) -> Path:
    if not os.path.isfile(NODE_LAUNCHER):
        raise RuntimeError(f"Node.js launcher not found: {NODE_LAUNCHER}")

    node_platform = get_node_platform_name()
    architecture = get_architecture_name()
    package_name = f"{NPM_PACKAGE_SCOPE}/{APPLICATION_NAME}-{node_platform}-{architecture}"
    with tempfile.TemporaryDirectory(prefix="crt-cpp-app-node-") as package_dir:
        app_dir = os.path.join(package_dir, "app")
        bin_dir = os.path.join(package_dir, "bin")
        with tempfile.TemporaryDirectory(prefix="crt-cpp-app-publish-") as publish_dir_str:
            publish_dir = Path(publish_dir_str)
            publish_application(publish_dir)
            shutil.copytree(publish_dir, app_dir, ignore=shutil.ignore_patterns("*.pdb"))
        os.makedirs(bin_dir)
        shutil.copy2(NODE_LAUNCHER, os.path.join(bin_dir, "crt-cpp-app.js"))

        package_json = {
            "name": package_name,
            "version": version,
            "description": "Create a C++ CMake project from the command line",
            "license": "MIT",
            "os": [node_platform],
            "cpu": [architecture],
            "bin": {APPLICATION_NAME: "bin/crt-cpp-app.js"},
            "files": ["app/", "bin/"],
        }
        with open(os.path.join(package_dir, "package.json"), "w", encoding="utf-8", newline="\n") as package_file:
            json.dump(package_json, package_file, indent=2)
            package_file.write("\n")

        try:
            result = subprocess.run(
                ["npm.cmd" if os.name == "nt" else "npm", "pack", "--json"],
                cwd=package_dir,
                capture_output=True,
                text=True,
                check=False,
            )
        except FileNotFoundError as error:
            raise RuntimeError("npm was not found; install Node.js to create an npm package") from error
        if result.returncode != 0:
            raise RuntimeError(result.stderr.strip() or "Failed to create the npm package")

        try:
            package_filename = json.loads(result.stdout)[0]["filename"]
        except (IndexError, json.JSONDecodeError, KeyError) as error:
            raise RuntimeError("npm pack did not report the generated package filename") from error

        source_path = os.path.join(package_dir, package_filename)
        target_path = os.path.join(dist_dir, package_filename)
        shutil.move(source_path, target_path)

    return Path(target_path)


def main():
    import argparse

    parser = argparse.ArgumentParser(description="Archive build output")
    parser.add_argument(
        "--zip",
        action="store_true",
        help="Package the self-contained application as a ZIP file")
    parser.add_argument(
        "--python",
        dest="python_package",
        action="store_true",
        help="Build a pip-installable Python wheel")
    parser.add_argument(
        "--nuget",
        action="store_true",
        help="Build a NuGet package for .NET projects")
    parser.add_argument(
        "--nodejs",
        action="store_true",
        help="Build an npm-installable Node.js package")
    parser.add_argument(
        "--all",
        action="store_true",
        help="Build the ZIP archive, Python wheel, NuGet package, and Node.js package")
    args = parser.parse_args()

    create_zip = args.zip or args.all
    create_python = args.python_package or args.all
    create_nuget = args.nuget or args.all
    create_nodejs = args.nodejs or args.all
    create_folder = not create_zip and not create_python and not create_nuget and not create_nodejs

    if (create_zip or create_folder) and not os.path.isfile(DOCUMENTATION_FILE):
        print(
            f"Error: documentation file not found at {DOCUMENTATION_FILE}",
            file=sys.stderr,
        )
        sys.exit(1)

    dist_dir = get_dist_dir()
    os.makedirs(dist_dir, exist_ok=True)
    version = read_version_info().version

    archived_path_list = []
    if create_folder:
        archived_path = archive_folder(version, dist_dir)
        archived_path_list.append(archived_path)
    if create_zip:
        archived_path = archive_zip(version, dist_dir)
        archived_path_list.append(archived_path)
    if create_python:
        archived_path = archive_python(version, dist_dir)
        archived_path_list.append(archived_path)
    if create_nuget:
        archived_path = archive_nuget(version, dist_dir)
        archived_path_list.append(archived_path)
    if create_nodejs:
        archived_path = archive_nodejs(version, dist_dir)
        archived_path_list.append(archived_path)

    if len(archived_path_list) == 0:
        return
    print("Archived:")
    for archived_path in archived_path_list:
        print(f"  - {archived_path}")


if __name__ == "__main__":
    main()
