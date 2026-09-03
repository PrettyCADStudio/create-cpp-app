import argparse
import glob
import json
import os
import platform
import shutil
import subprocess
import sys
import tempfile
import zipfile

PROJECT_FILE = os.path.join("src", "crt-cpp-app", "crt-cpp-app.csproj")
DIST_DIR = "dist"
DOCUMENTATION_FILE = os.path.join("docs", "doc.md")
APPLICATION_NAME = "crt-cpp-app"
NPM_PACKAGE_SCOPE = "@prettycadstudio"
NODE_LAUNCHER = os.path.join("nodejs", "crt-cpp-app.js")


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
        [sys.executable, "-m", "pip", "wheel", ".", "--wheel-dir", DIST_DIR, "--no-deps"],
    )
    if result.returncode != 0:
        raise RuntimeError("Failed to build the Python wheel")

    wheels = glob.glob(os.path.join(DIST_DIR, f"crt_cpp_app-{version}-*.whl"))
    if not wheels:
        raise RuntimeError("Python wheel build completed but no wheel was found")

    wheel_path = max(wheels, key=os.path.getmtime)
    print(f"Archived to {wheel_path}", file=sys.stderr)
    print(wheel_path)


def archive_nuget(version):
    """Build a NuGet package for use by .NET projects."""
    result = subprocess.run(
        ["dotnet", "pack", PROJECT_FILE, "-c", "Release", "--output", DIST_DIR]
    )
    if result.returncode != 0:
        raise RuntimeError("Failed to build the NuGet package")

    packages = glob.glob(os.path.join(DIST_DIR, f"{APPLICATION_NAME}.{version}.nupkg"))
    if not packages:
        raise RuntimeError("NuGet pack completed but no package was found")

    package_path = max(packages, key=os.path.getmtime)
    print(f"Archived to {package_path}", file=sys.stderr)
    print(package_path)


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


def archive_nodejs(version):
    """Build a platform-specific npm package containing the C# application."""
    if not os.path.isfile(NODE_LAUNCHER):
        raise RuntimeError(f"Node.js launcher not found: {NODE_LAUNCHER}")

    node_platform = get_node_platform_name()
    architecture = get_architecture_name()
    package_name = f"{NPM_PACKAGE_SCOPE}/{APPLICATION_NAME}-{node_platform}-{architecture}"
    with tempfile.TemporaryDirectory(prefix="crt-cpp-app-node-") as package_dir:
        app_dir = os.path.join(package_dir, "app")
        bin_dir = os.path.join(package_dir, "bin")
        with tempfile.TemporaryDirectory(prefix="crt-cpp-app-publish-") as publish_dir:
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
        target_path = os.path.join(DIST_DIR, package_filename)
        shutil.move(source_path, target_path)

    print(f"Archived to {target_path}", file=sys.stderr)
    print(target_path)


def main():
    parser = argparse.ArgumentParser(description="Archive build output")
    parser.add_argument("--zip", action="store_true", help="Package the self-contained application as a ZIP file")
    parser.add_argument("--python", dest="python_package", action="store_true", help="Build a pip-installable Python wheel")
    parser.add_argument("--nuget", action="store_true", help="Build a NuGet package for .NET projects")
    parser.add_argument("--nodejs", action="store_true", help="Build an npm-installable Node.js package")
    parser.add_argument("--all", action="store_true", help="Build the ZIP archive, Python wheel, NuGet package, and Node.js package")
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

    os.makedirs(DIST_DIR, exist_ok=True)

    version = read_version()

    if create_zip or create_folder:
        platform_name = get_platform_name()
        architecture_name = get_architecture_name()
        name = f"{APPLICATION_NAME}-v{version}-{platform_name}-{architecture_name}"
        with tempfile.TemporaryDirectory(prefix="crt-cpp-app-publish-") as publish_dir:
            publish_application(publish_dir)
            files = collect_files(publish_dir)

            if create_zip:
                archive_zip(name, files)
            elif create_folder:
                archive_folder(name, files)

    if create_python:
        archive_python(version)

    if create_nuget:
        archive_nuget(version)

    if create_nodejs:
        archive_nodejs(version)


if __name__ == "__main__":
    main()
