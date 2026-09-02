import argparse
import os
import shutil
import tempfile

CLEAN_DIRS = [
    "bin",
    os.path.join("src", "create-cpp-app", "obj"),
    os.path.join("test", "create-cpp-app.Tests", "obj"),
]


def remove_dir(path):
    if os.path.exists(path):
        print(f"  Removing {path}")
        shutil.rmtree(path)
    else:
        print(f"  Skipping {path} (not found)")


def remove_project_temp_dirs():
    temp_dir = tempfile.gettempdir()
    prefix = "create-cpp-app-"

    for entry in os.scandir(temp_dir):
        if entry.name.startswith(prefix) and entry.is_dir(follow_symlinks=False):
            remove_dir(entry.path)


def main():
    parser = argparse.ArgumentParser(description="Clean build artifacts")
    parser.add_argument(
        "--dist", action="store_true", help="Also remove the dist folder"
    )
    args = parser.parse_args()

    print("Cleaning build artifacts...")
    for d in CLEAN_DIRS:
        remove_dir(d)

    print("Cleaning create-cpp-app system temporary directories...")
    remove_project_temp_dirs()

    if args.dist:
        remove_dir("dist")

    print("Done.")


if __name__ == "__main__":
    main()
