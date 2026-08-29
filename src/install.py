import argparse
import os
import platform
import shutil
import sys

INSTALL_DIR = os.path.join(os.path.expanduser("~"), ".create-cpp-app")


def clean_install_dir():
    if os.path.exists(INSTALL_DIR):
        print(f"[2/4] Removing old installation at {INSTALL_DIR}")
        shutil.rmtree(INSTALL_DIR)
    else:
        print(f"[2/4] No existing installation found")


def copy_files(source_dir):
    print(f"[3/4] Copying files to {INSTALL_DIR}")
    os.makedirs(INSTALL_DIR, exist_ok=True)

    for name in os.listdir(source_dir):
        if name == "install.py":
            continue
        src = os.path.join(source_dir, name)
        dst = os.path.join(INSTALL_DIR, name)
        if os.path.isdir(src):
            shutil.copytree(src, dst)
        else:
            shutil.copy2(src, dst)


def add_to_path():
    print(f"[4/4] Adding {INSTALL_DIR} to PATH")

    system = platform.system()

    if system == "Windows":
        import subprocess
        current = subprocess.check_output(
            ["powershell", "-Command", "[Environment]::GetEnvironmentVariable('Path','User')"],
            text=True,
        ).strip()
        if INSTALL_DIR not in current:
            new_path = current + ";" + INSTALL_DIR if current else INSTALL_DIR
            subprocess.run(
                ["powershell", "-Command",
                 f"[Environment]::SetEnvironmentVariable('Path','{new_path}','User')"],
                check=True,
            )
        print(f"  Windows: added to User PATH (restart terminal to take effect)")
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
    parser = argparse.ArgumentParser(description="Install create-cpp-app")
    parser.add_argument(
        "--source",
        default=os.path.dirname(os.path.abspath(__file__)),
        help="Source directory to install from (default: script directory)",
    )
    args = parser.parse_args()

    print(f"[1/4] Installing create-cpp-app")

    clean_install_dir()
    copy_files(args.source)
    add_to_path()

    print()
    print("Installation complete!")
    print(f"  Installed to: {INSTALL_DIR}")
    print(f"  Restart your terminal, then run: create-cpp-app")


if __name__ == "__main__":
    main()
