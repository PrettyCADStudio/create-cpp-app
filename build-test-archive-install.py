import subprocess
import sys


def run(cmd):
    print(f"{'=' * 40}")
    print(f"{' '.join(cmd)}")
    print(f"{'=' * 40}")
    result = subprocess.run(cmd)
    if result.returncode != 0:
        print(f"\nFailed (exit code {result.returncode}), aborting.")
        sys.exit(result.returncode)
    print()


def run_capture(cmd):
    print(f"{'=' * 40}")
    print(f"{' '.join(cmd)}")
    print(f"{'=' * 40}")
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        if result.stderr:
            print(result.stderr, file=sys.stderr)
        print(f"\nFailed (exit code {result.returncode}), aborting.")
        sys.exit(result.returncode)
    if result.stderr:
        print(result.stderr, end="")
    print()
    # Commands that produce a machine-readable result must print it as their
    # final stdout line. Keep this resilient to incidental informational output.
    lines = [line for line in result.stdout.splitlines() if line.strip()]
    if not lines:
        print("Command produced no output.", file=sys.stderr)
        sys.exit(1)
    return lines[-1]


def main():
    print("Step 1/4: Build")
    run([sys.executable, "build-dotnet.py"])

    print("Step 2/4: Test")
    run(["dotnet", "test", "crt-cpp-app.slnx", "--verbosity", "normal"])
    print("All tests passed.\n")

    print("Step 3/4: Archive")
    archive_path = run_capture([sys.executable, "archive.py"])

    print("Step 4/4: Install")
    run([sys.executable, "src/install.py", "--source", archive_path])

    print("All steps completed successfully.")


if __name__ == "__main__":
    main()
