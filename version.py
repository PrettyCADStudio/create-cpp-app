"""Read or update the version shared by the .NET, Python, npm, and NuGet packages."""

from __future__ import annotations

import argparse
import re
import sys
import tempfile
from pathlib import Path


PROJECT_FILE = Path(__file__).parent / "src" / "crt-cpp-app" / "crt-cpp-app.csproj"
VERSION_TAGS = ("Version", "AssemblyVersion", "FileVersion", "InformationalVersion")
VERSION_PATTERN = re.compile(r"\d+\.\d+\.\d+(?:\.\d+)?")


def read_project_file() -> str:
    try:
        return PROJECT_FILE.read_bytes().decode("utf-8")
    except FileNotFoundError as error:
        raise RuntimeError(f"Project file was not found: {PROJECT_FILE}") from error


def read_version(content: str) -> str:
    match = re.search(r"<Version>([^<]+)</Version>", content)
    if match is None:
        raise RuntimeError(f"Version element was not found in {PROJECT_FILE}")
    return match.group(1)


def update_versions(content: str, version: str) -> str:
    for tag in VERSION_TAGS:
        pattern = re.compile(rf"(<{tag}>)([^<]*)(</{tag}>)")
        content, count = pattern.subn(rf"\g<1>{version}\g<3>", content)
        if count != 1:
            raise RuntimeError(
                f"Expected exactly one <{tag}> element in {PROJECT_FILE}, found {count}"
            )
    return content


def write_project_file(content: str) -> None:
    with tempfile.NamedTemporaryFile(
        mode="wb", dir=PROJECT_FILE.parent, prefix=f".{PROJECT_FILE.name}.", delete=False
    ) as temporary_file:
        temporary_path = Path(temporary_file.name)
        temporary_file.write(content.encode("utf-8"))

    try:
        temporary_path.replace(PROJECT_FILE)
    except Exception:
        temporary_path.unlink(missing_ok=True)
        raise


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Read or update the version defined in crt-cpp-app.csproj."
    )
    parser.add_argument(
        "--update",
        metavar="VERSION",
        help="Set Version, AssemblyVersion, FileVersion, and InformationalVersion.",
    )
    args = parser.parse_args()

    try:
        content = read_project_file()
        if args.update is None:
            print(read_version(content))
            return 0

        if VERSION_PATTERN.fullmatch(args.update) is None:
            parser.error("--update must be a numeric version with three or four parts, such as 0.1.9")

        write_project_file(update_versions(content, args.update))
        print(f"Updated version to {args.update}")
        return 0
    except RuntimeError as error:
        print(f"Error: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
