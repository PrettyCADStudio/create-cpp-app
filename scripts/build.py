# coding: utf-8

from shared import run_command, get_sln_file, path_to_str


def build_solution(config: str):
    solution_file = get_sln_file()
    command_args = ["dotnet", "build", path_to_str(solution_file), "-c", config]
    result = run_command(command_args)
    return result


def main():
    import sys
    import argparse

    parser = argparse.ArgumentParser(description="Build crt-cpp-app solution")
    parser.add_argument(
        "--config",
        choices=["Debug", "Release"],
        default="Release",
        help="Build configuration (default: Release)",
    )
    args = parser.parse_args()

    result = build_solution(args.config)
    sys.exit(result.returncode)


if __name__ == "__main__":
    main()
