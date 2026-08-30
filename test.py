import subprocess
import sys
import glob
import shutil


def run(cmd):
    print("=" * 40)
    print(" ".join(cmd))
    print("=" * 40)
    result = subprocess.run(cmd)
    if result.returncode != 0:
        print(f"\nTests failed (exit code {result.returncode})")
        sys.exit(result.returncode)
    print()


def find_solution():
    sols = glob.glob("*.sln*")
    if not sols:
        sols = glob.glob("**/*.sln*", recursive=True)
    return sols[0] if sols else None


def main():
    sln = find_solution()
    if not sln:
        print("No .sln or .slnx solution file found in the repository.")
        sys.exit(1)

    if not shutil.which("dotnet"):
        print("dotnet CLI not found in PATH. Please install the .NET SDK or ensure 'dotnet' is on PATH.")
        sys.exit(1)

    # Run the .NET test runner and stream output to the console so results are visible.
    cmd = ["dotnet", "test", sln, "--verbosity", "normal"]
    run(cmd)


if __name__ == "__main__":
    main()
