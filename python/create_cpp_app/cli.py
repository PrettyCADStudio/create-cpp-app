"""Run the self-contained .NET implementation bundled in this wheel."""

from __future__ import annotations

import os
import stat
import subprocess
import sys
from pathlib import Path


def _application_path() -> Path:
    executable_name = "create-cpp-app.exe" if os.name == "nt" else "create-cpp-app"
    application = Path(__file__).resolve().parent / "_app" / executable_name
    if not application.is_file():
        raise RuntimeError(
            "The bundled create-cpp-app application is missing. "
            "Reinstall the package for this platform."
        )
    return application


def main() -> int:
    """Invoke the bundled command and return its exit code."""
    try:
        application = _application_path()
        if os.name != "nt":
            application.chmod(application.stat().st_mode | stat.S_IXUSR)
        return subprocess.run([str(application), *sys.argv[1:]], check=False).returncode
    except (OSError, RuntimeError) as error:
        print(f"create-cpp-app: {error}", file=sys.stderr)
        return 1
