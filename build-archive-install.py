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
    return result.stdout.strip()


def main():
    print("Step 1/3: Build")
    run([sys.executable, "build.py"])

    print("Step 2/3: Archive")
    archive_path = run_capture([sys.executable, "archive.py"])

    print("Step 3/3: Install")
    run([sys.executable, "src/install.py", "--source", archive_path])

    print("All steps completed successfully.")


if __name__ == "__main__":
    main()
