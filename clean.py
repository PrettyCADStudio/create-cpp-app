import argparse
import os
import shutil

CLEAN_DIRS = [
    "bin",
    "temp",
    os.path.join("src", "create-cpp-app", "obj"),
]


def remove_dir(path):
    if os.path.exists(path):
        print(f"  Removing {path}")
        shutil.rmtree(path)
    else:
        print(f"  Skipping {path} (not found)")


def main():
    parser = argparse.ArgumentParser(description="Clean build artifacts")
    parser.add_argument(
        "--dist", action="store_true", help="Also remove the dist folder"
    )
    args = parser.parse_args()

    print("Cleaning build artifacts...")
    for d in CLEAN_DIRS:
        remove_dir(d)

    if args.dist:
        remove_dir("dist")

    print("Done.")


if __name__ == "__main__":
    main()
