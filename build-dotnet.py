import argparse
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description="Build crt-cpp-app solution")
    parser.add_argument(
        "--config",
        choices=["Debug", "Release"],
        default="Release",
        help="Build configuration (default: Release)",
    )
    args = parser.parse_args()

    result = subprocess.run(
        ["dotnet", "build", "crt-cpp-app.slnx", "-c", args.config]
    )
    sys.exit(result.returncode)


if __name__ == "__main__":
    main()
