import argparse
import os
import platform
import shutil
import subprocess
import sys
import zipfile

PROJECT_FILE = os.path.join("src", "create-cpp-app", "create-cpp-app.csproj")
BUILD_DIR = os.path.join("bin", "Release", "net10.0")
DIST_DIR = "dist"
DOCUMENTATION_FILE = os.path.join("docs", "doc.md")
APPLICATION_NAME = "create-cpp-app"


def read_version():
    result = subprocess.run(
        [
            "dotnet",
            "msbuild",
            PROJECT_FILE,
            "-nologo",
            "-getProperty:Version",
        ],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        raise RuntimeError(
            result.stderr.strip() or f"Failed to read Version from {PROJECT_FILE}"
        )

    version = result.stdout.strip()
    if not version:
        raise RuntimeError(f"Version property is empty in {PROJECT_FILE}")
    return version


def collect_files():
    files = []
    for root, _, filenames in os.walk(BUILD_DIR):
        for f in filenames:
            if f.endswith(".pdb"):
                continue
            file_path = os.path.join(root, f)
            arcname = os.path.relpath(file_path, BUILD_DIR)
            files.append((file_path, arcname))

    install_script = os.path.join("src", "install.py")
    if os.path.isfile(install_script):
        files.append((install_script, "install.py"))

    files.append((DOCUMENTATION_FILE, "doc.md"))

    return files


def get_platform_name():
    system_names = {
        "Darwin": "macos",
        "Linux": "linux",
        "Windows": "windows",
    }
    system = platform.system()
    return system_names.get(system, system.lower())


def get_architecture_name():
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


def archive_zip(name, files):
    zip_path = os.path.join(DIST_DIR, f"{name}.zip")
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
        for file_path, arcname in files:
            zf.write(file_path, arcname)
    print(f"Archived to {zip_path}", file=sys.stderr)
    print(zip_path)


def archive_folder(name, files):
    folder_path = os.path.join(DIST_DIR, name)
    os.makedirs(folder_path, exist_ok=True)
    for file_path, arcname in files:
        dest = os.path.join(folder_path, arcname)
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        shutil.copy2(file_path, dest)
    print(f"Archived to {folder_path}", file=sys.stderr)
    print(folder_path)


def main():
    parser = argparse.ArgumentParser(description="Archive build output")
    parser.add_argument("--zip", action="store_true", help="Package as a zip file")
    args = parser.parse_args()

    if not os.path.isdir(BUILD_DIR):
        print(f"Error: build output not found at {BUILD_DIR}", file=sys.stderr)
        print("Run 'python build.py' first.", file=sys.stderr)
        sys.exit(1)

    if not os.path.isfile(DOCUMENTATION_FILE):
        print(
            f"Error: documentation file not found at {DOCUMENTATION_FILE}",
            file=sys.stderr,
        )
        sys.exit(1)

    os.makedirs(DIST_DIR, exist_ok=True)

    version = read_version()
    platform_name = get_platform_name()
    architecture_name = get_architecture_name()
    name = f"{APPLICATION_NAME}-v{version}-{platform_name}-{architecture_name}"
    files = collect_files()

    if args.zip:
        archive_zip(name, files)
    else:
        archive_folder(name, files)


if __name__ == "__main__":
    main()
