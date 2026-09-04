# coding: utf-8

import sys

from shared import run_command, get_sln_file, path_to_str


def run_test():
    solution_file = get_sln_file()
    args = ["dotnet", "test", path_to_str(solution_file), "--verbosity", "normal"]
    return run_command(args)


def main():
    result = run_test()
    if 0 != result.returncode:
        print()
        print(f"Tests failed (exit code {result.returncode})")
        sys.exit(result.returncode)


if __name__ == "__main__":
    main()
