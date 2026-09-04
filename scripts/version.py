"""Read, update, and verify release version metadata in the project file."""

from __future__ import annotations

import re
import sys
import tempfile
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path

from shared import get_proj_file


VERSION_FIELDS = (
    "Version",
    "AssemblyVersion",
    "FileVersion",
    "InformationalVersion",
)
VERSION_PATTERN = re.compile(r"\d+\.\d+\.\d+(?:\.\d+)?")


@dataclass
class VersionInfo:

    version: str
    assembly_version: str
    file_version: str
    informational_version: str

    def validate(self) -> bool:
        values = (
            self.version,
            self.assembly_version,
            self.file_version,
            self.informational_version,
        )
        return all(VERSION_PATTERN.fullmatch(value) for value in values) and len(set(values)) == 1

    def update(self, version: str) -> None:
        if VERSION_PATTERN.fullmatch(version) is None:
            raise ValueError(
                "Version must have three or four numeric parts, such as 0.1.9."
            )
        self.version = version
        self.assembly_version = version
        self.file_version = version
        self.informational_version = version

    def __str__(self) -> str:
        return self.version


def read_version_info() -> VersionInfo:
    project_file = get_proj_file()
    try:
        root = ET.parse(project_file).getroot()
    except FileNotFoundError as error:
        raise RuntimeError(f"Project file was not found: {project_file}") from error
    except ET.ParseError as error:
        raise RuntimeError(f"Project file is not valid XML: {project_file}") from error

    values: dict[str, str] = {}
    for field in VERSION_FIELDS:
        elements = [
            element
            for element in root.iter()
            if element.tag.rsplit("}", 1)[-1] == field
        ]
        if len(elements) != 1 or elements[0].text is None:
            raise RuntimeError(
                f"Expected exactly one <{field}> element in {project_file}, found {len(elements)}"
            )
        values[field] = elements[0].text.strip()

    return VersionInfo(
        version=values["Version"],
        assembly_version=values["AssemblyVersion"],
        file_version=values["FileVersion"],
        informational_version=values["InformationalVersion"],
    )


def update_project_version(version: str) -> VersionInfo:
    info = read_version_info()
    info.update(version)

    project_file = get_proj_file()
    content = project_file.read_text(encoding="utf-8")
    replacements = {
        "Version": info.version,
        "AssemblyVersion": info.assembly_version,
        "FileVersion": info.file_version,
        "InformationalVersion": info.informational_version,
    }
    for field, value in replacements.items():
        pattern = re.compile(rf"(<{field}>)[^<]*(</{field}>)")
        content, count = pattern.subn(rf"\g<1>{value}\g<2>", content)
        if count != 1:
            raise RuntimeError(
                f"Expected exactly one <{field}> element in {project_file}, found {count}"
            )

    with tempfile.NamedTemporaryFile(
        mode="w", encoding="utf-8", dir=project_file.parent,
        prefix=f".{project_file.name}.", delete=False,
    ) as temporary_file:
        temporary_path = Path(temporary_file.name)
        temporary_file.write(content)

    try:
        temporary_path.replace(project_file)
    except Exception:
        temporary_path.unlink(missing_ok=True)
        raise

    return info


def main() -> int:
    import argparse

    parser = argparse.ArgumentParser(
        description="Read, update, or verify the version defined in crt-cpp-app.csproj."
    )
    action = parser.add_mutually_exclusive_group()
    action.add_argument(
        "--update",
        metavar="VERSION",
        help="Set Version, AssemblyVersion, FileVersion, and InformationalVersion.",
    )
    action.add_argument(
        "--verify",
        action="store_true",
        help="Verify that all version fields are valid and identical.",
    )
    args = parser.parse_args()

    if args.update is not None:
        info = update_project_version(args.update)
        print(f"Updated version to {info.version}")
        return 0

    info = read_version_info()
    if args.verify:
        if not info.validate():
            print("Error: version fields must be valid and identical.", file=sys.stderr)
            return 1
        print(f"Verified version {info.version}")
        return 0

    print(info.version)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
