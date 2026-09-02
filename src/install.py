import argparse
import os
import platform
import shutil
import sys
import tempfile
import uuid

INSTALL_DIR = os.path.join(os.path.expanduser("~"), ".crt-cpp-app")


def validate_source(source_dir):
    if not os.path.isdir(source_dir):
        raise ValueError(f"Source directory does not exist: {source_dir}")
    if not os.listdir(source_dir):
        raise ValueError(f"Source directory is empty: {source_dir}")


def copy_files(source_dir, destination_dir):
    os.makedirs(destination_dir, exist_ok=True)

    for name in os.listdir(source_dir):
        if name == "install.py":
            continue
        src = os.path.join(source_dir, name)
        dst = os.path.join(destination_dir, name)
        if os.path.isdir(src):
            shutil.copytree(src, dst)
        else:
            shutil.copy2(src, dst)


def replace_installation(staging_dir):
    parent_dir = os.path.dirname(INSTALL_DIR)
    backup_dir = os.path.join(parent_dir, f".crt-cpp-app.backup-{uuid.uuid4().hex}")
    had_previous_installation = os.path.exists(INSTALL_DIR)

    try:
        if had_previous_installation:
            print(f"[3/4] Backing up existing installation at {INSTALL_DIR}")
            os.replace(INSTALL_DIR, backup_dir)
        else:
            print("[3/4] No existing installation found")

        os.replace(staging_dir, INSTALL_DIR)
    except Exception:
        if had_previous_installation and os.path.exists(backup_dir) and not os.path.exists(INSTALL_DIR):
            os.replace(backup_dir, INSTALL_DIR)
        raise
    else:
        if os.path.exists(backup_dir):
            shutil.rmtree(backup_dir)


def add_to_path():
    print(f"[4/4] Adding {INSTALL_DIR} to PATH")

    system = platform.system()

    if system == "Windows":
        import winreg

        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, "Environment", 0, winreg.KEY_READ | winreg.KEY_WRITE) as key:
            try:
                current, value_type = winreg.QueryValueEx(key, "Path")
            except FileNotFoundError:
                current, value_type = "", winreg.REG_EXPAND_SZ

            existing_paths = {os.path.normcase(os.path.normpath(item)) for item in current.split(";") if item}
            if os.path.normcase(os.path.normpath(INSTALL_DIR)) not in existing_paths:
                new_path = current + ";" + INSTALL_DIR if current else INSTALL_DIR
                winreg.SetValueEx(key, "Path", 0, value_type, new_path)
                print("  Windows: added to User PATH (restart terminal to take effect)")
            else:
                print("  Windows: already in User PATH")
        return

    shell = os.path.basename(os.environ.get("SHELL", ""))
    rc_map = {
        "bash": ".bashrc",
        "zsh": ".zshrc",
        "fish": "config.fish",
    }
    rc_file = rc_map.get(shell)
    if not rc_file:
        print(f"  Unknown shell '{shell}', please add {INSTALL_DIR} to PATH manually")
        return

    if shell == "fish":
        rc_path = os.path.join(os.path.expanduser("~"), ".config", "fish", rc_file)
        line = f'set -gx PATH {INSTALL_DIR} $PATH\n'
    else:
        rc_path = os.path.join(os.path.expanduser("~"), rc_file)
        line = f'export PATH="{INSTALL_DIR}:$PATH"\n'

    if os.path.exists(rc_path):
        with open(rc_path, "r") as f:
            if INSTALL_DIR in f.read():
                print(f"  {INSTALL_DIR} already in {rc_path}")
                return

    os.makedirs(os.path.dirname(rc_path), exist_ok=True)
    with open(rc_path, "a") as f:
        f.write(line)
    print(f"  Appended to {rc_path}")


def main():
    parser = argparse.ArgumentParser(description="Install crt-cpp-app")
    parser.add_argument(
        "--source",
        default=os.path.dirname(os.path.abspath(__file__)),
        help="Source directory to install from (default: script directory)",
    )
    args = parser.parse_args()

    source_dir = os.path.abspath(args.source)
    try:
        validate_source(source_dir)
    except ValueError as error:
        parser.error(str(error))

    print(f"[1/4] Validated source: {source_dir}")
    parent_dir = os.path.dirname(INSTALL_DIR)
    os.makedirs(parent_dir, exist_ok=True)
    staging_dir = tempfile.mkdtemp(prefix=".crt-cpp-app.staging-", dir=parent_dir)
    try:
        print(f"[2/4] Copying files to temporary directory")
        copy_files(source_dir, staging_dir)
        replace_installation(staging_dir)
    except Exception:
        if os.path.exists(staging_dir):
            shutil.rmtree(staging_dir)
        raise
    add_to_path()

    print()
    print("Installation complete!")
    print(f"  Installed to: {INSTALL_DIR}")
    print(f"  Restart your terminal, then run: crt-cpp-app")


if __name__ == "__main__":
    main()
