# coding: utf-8

import os
import shutil
import tempfile
from pathlib import Path
from typing import Sequence

from shared import (
    get_bin_dir, get_build_dir, get_dist_dir, get_proj_dir, get_test_proj_dir, get_python_dir,
    APPLICATION_NAME
)


def collect_dirs_to_remove(include_dist_dir: bool) -> Sequence[Path]:
    python_dir = get_python_dir()
    result = [
        get_bin_dir(),
        get_build_dir(),
        get_proj_dir() / "obj",
        get_test_proj_dir() / "obj",
        python_dir / "build",
    ]

    for sub in python_dir.iterdir():
        if sub.is_dir() and sub.name.endswith(".egg-info"):
            result.append(sub)

    if include_dist_dir:
        result.append(get_dist_dir())

    temp_dir = tempfile.gettempdir()
    prefix = f"{APPLICATION_NAME}-"
    for entry in os.scandir(temp_dir):
        if entry.name.startswith(prefix) and entry.is_dir(follow_symlinks=False):
            result.append(Path(entry.path))

    return result


def remove_dir(path):
    if os.path.exists(path):
        print(f"  Removing {path}")
        shutil.rmtree(path)
    else:
        print(f"  Skipping {path} (not found)")


def remove_dirs(include_dist_dir: bool):
    dirs_to_remove = collect_dirs_to_remove(include_dist_dir)
    for dir_to_remove in dirs_to_remove:
        remove_dir(dir_to_remove)


def main():
    import argparse

    parser = argparse.ArgumentParser(description="Clean build artifacts")
    parser.add_argument(
        "--include-dist-dir",
        action="store_true",
        help="Also remove the dist folder"
    )
    args = parser.parse_args()

    remove_dirs(args.include_dist_dir)


if __name__ == "__main__":
    main()
