# Copilot instructions for create-cpp-app

## Build, test, and validation commands

Use the repository scripts for normal validation:

```bash
# Build the C# CLI (Release by default)
python build-dotnet.py

# Build the CLI in Debug
python build-dotnet.py --config Debug

# Run every fixture-based test
python test.py

# Run all fixture cases through the core theory
dotnet test create-cpp-app.slnx --filter "Create_MatchesFixture"

# Run one fixture case by its generated project name
dotnet test create-cpp-app.slnx --filter "Name~TestProjectAllOptions"
```

There is no separate lint command. Tests require CMake in addition to the .NET SDK because every fixture is configured and built as a generated C++ project.

## High-level architecture

This is a .NET 10 interactive CLI that generates complete CMake-based C++ project skeletons.

- `Program.cs` defines the `System.CommandLine` root command, including `--force`; the framework supplies `--help` and `--version`.
- `UserInteraction.cs` collects the project name, C++17/C++20 choice, and optional `inc`, `res`, `3rd`, and `patch` directories.
- `CMakeProjectSettings.cs` contains the selected options and derives the generated project paths from the current working directory.
- `CMakeProjectCreator.cs` is the generator. It writes the root CMake file, the App executable, Static and Dynamic libraries, their public/private files, and the selected optional directories.
- `test/create-cpp-app.Tests/CMakeProjectCreatorTests.cs` discovers every immediate subdirectory in `test/fixtures` that contains `create-cpp-app.json`. It deserializes that configuration, invokes the generator, compares the generated tree and file contents to the fixture, then runs CMake configure and Release build.

## Project-specific conventions

- Add generator scenarios by creating a new `test/fixtures/<case>/create-cpp-app.json` and its complete expected generated tree. Do not add one-off assertion tests for option combinations that fixture discovery can cover.
- Fixture file comparisons are byte-for-byte at the decoded-text level, excluding only `create-cpp-app.json`; generator formatting, blank lines, and final newlines are therefore part of the contract.
- Generated files use LF (`\n`) line endings. Preserve this for fixtures and new generator output.
- Generated C++ source uses four spaces per indentation level. Keep each generated output line in a distinct `AppendLine` call rather than embedding multi-line source text in a single call.
- Optional `res`, `3rd`, and `patch` directories are represented by empty `.keep` files so fixture comparisons retain them.
- The test runner writes generated projects and CMake build directories to a unique directory under the system temporary directory, then removes only that directory during cleanup. Do not rely on artifacts from earlier runs.
- Version metadata is defined in `src/create-cpp-app/create-cpp-app.csproj`. Keep `Version`, `AssemblyVersion`, `FileVersion`, and `InformationalVersion` aligned so the built-in CLI `--version` output remains correct.
