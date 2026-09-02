import argparse
import glob
import os
import platform
import shutil
import subprocess
import sys
import tempfile
import zipfile

PROJECT_FILE = os.path.join("src", "create-cpp-app", "create-cpp-app.csproj")
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


def publish_application(output_dir):
    # archive.py's stdout is consumed by build-test-archive-install.py as the
    # archive path, so publish progress must not be written there.
    result = subprocess.run(
        [
            "dotnet", "publish", PROJECT_FILE, "-c", "Release",
            "-r", get_runtime_identifier(), "--self-contained", "true",
            "--output", output_dir,
        ],
        stdout=sys.stderr,
    )
    if result.returncode != 0:
        raise RuntimeError("Failed to publish a self-contained release package")


def collect_files(build_dir):
    files = []
    for root, _, filenames in os.walk(build_dir):
        for f in filenames:
            if f.endswith(".pdb"):
                continue
            file_path = os.path.join(root, f)
            arcname = os.path.relpath(file_path, build_dir)
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
    with tempfile.NamedTemporaryFile(prefix=f".{name}-", suffix=".zip", dir=DIST_DIR, delete=False) as tmp:
        temp_zip_path = tmp.name
    with zipfile.ZipFile(temp_zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
        for file_path, arcname in files:
            zf.write(file_path, arcname)
    os.replace(temp_zip_path, zip_path)
    print(f"Archived to {zip_path}", file=sys.stderr)
    print(zip_path)


def archive_folder(name, files):
    folder_path = os.path.join(DIST_DIR, name)
    staging_path = tempfile.mkdtemp(prefix=f".{name}-", dir=DIST_DIR)
    for file_path, arcname in files:
        dest = os.path.join(staging_path, arcname)
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        shutil.copy2(file_path, dest)
    if os.path.exists(folder_path):
        shutil.rmtree(folder_path)
    os.replace(staging_path, folder_path)
    print(f"Archived to {folder_path}", file=sys.stderr)
    print(folder_path)


def archive_python(version):
    """Build a platform-specific wheel containing the self-contained C# CLI."""
    result = subprocess.run(
        [sys.executable, "-m", "build", "--wheel", "--outdir", DIST_DIR]
    )
    if result.returncode != 0:
        raise RuntimeError("Failed to build the Python wheel")

    wheels = glob.glob(os.path.join(DIST_DIR, f"create_cpp_app-{version}-*.whl"))
    if not wheels:
        raise RuntimeError("Python wheel build completed but no wheel was found")

    wheel_path = max(wheels, key=os.path.getmtime)
    print(f"Archived to {wheel_path}", file=sys.stderr)
    print(wheel_path)


def main():
    parser = argparse.ArgumentParser(description="Archive build output")
    parser.add_argument("--zip", action="store_true", help="Package the self-contained application as a ZIP file")
    parser.add_argument("--python", dest="python_package", action="store_true", help="Build a pip-installable Python wheel")
    parser.add_argument("--all", action="store_true", help="Build both the ZIP archive and Python wheel")
    args = parser.parse_args()

    create_zip = args.zip or args.all
    create_python = args.python_package or args.all
    create_folder = not create_zip and not create_python

    if (create_zip or create_folder) and not os.path.isfile(DOCUMENTATION_FILE):
        print(
            f"Error: documentation file not found at {DOCUMENTATION_FILE}",
            file=sys.stderr,
        )
        sys.exit(1)

    os.makedirs(DIST_DIR, exist_ok=True)

    version = read_version()

    if create_zip or create_folder:
        platform_name = get_platform_name()
        architecture_name = get_architecture_name()
        name = f"{APPLICATION_NAME}-v{version}-{platform_name}-{architecture_name}"
        with tempfile.TemporaryDirectory(prefix="create-cpp-app-publish-") as publish_dir:
            publish_application(publish_dir)
            files = collect_files(publish_dir)

            if create_zip:
                archive_zip(name, files)
            elif create_folder:
                archive_folder(name, files)

    if create_python:
        archive_python(version)


if __name__ == "__main__":
    main()
