import os
import sys
import zipfile
from datetime import datetime

VERSION = "0.0.1"
BUILD_DIR = os.path.join("bin", "Release", "net10.0")
DIST_DIR = "dist"


def main():
    if not os.path.isdir(BUILD_DIR):
        print(f"Error: build output not found at {BUILD_DIR}", file=sys.stderr)
        print("Run 'python build.py' first.", file=sys.stderr)
        sys.exit(1)

    os.makedirs(DIST_DIR, exist_ok=True)

    timestamp = datetime.now().strftime("%Y-%m-%d-%H-%M-%S")
    zip_name = f"{VERSION}-{timestamp}.zip"
    zip_path = os.path.join(DIST_DIR, zip_name)

    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
        for root, _, files in os.walk(BUILD_DIR):
            for f in files:
                if f.endswith(".pdb"):
                    continue
                file_path = os.path.join(root, f)
                arcname = os.path.relpath(file_path, BUILD_DIR)
                zf.write(file_path, arcname)

        install_script = os.path.join("src", "install.py")
        if os.path.isfile(install_script):
            zf.write(install_script, "install.py")

    print(f"Archived to {zip_path}")


if __name__ == "__main__":
    main()
