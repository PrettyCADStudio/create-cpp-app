# Copilot instructions for create-cpp-app

## Build, test, and validation commands

Use the repository's actual entry points rather than ad hoc commands:

```bash
# Build the CLI itself
python build.py

# Build Debug configuration if needed
python build.py --config Debug

# Run the full test suite
python test.py

# Run a single .NET test target
dotnet test create-cpp-app.slnx --filter "Create_MatchesFixture"

# Run a single fixture case by name (example)
dotnet test create-cpp-app.slnx --filter "Name~TestProject"
```

The test runner is intentionally fixture-driven: `test/create-cpp-app.Tests/CMakeProjectCreatorTests.cs` generates sample projects and validates both file content and real CMake configure/build steps.

## High-level architecture

This repository is a C++ project generator, not a library with a long runtime flow. The main pieces are:

- `src/create-cpp-app/Program.cs`: command-line entry point using `System.CommandLine`
- `src/create-cpp-app/UserInteraction.cs`: interactive prompts for project name, C++ standard, and `inc` option
- `src/create-cpp-app/CMakeProjectSettings.cs`: settings model used by the generator
- `src/create-cpp-app/CMakeProjectCreator.cs`: writes the generated project tree and CMake files
- `test/create-cpp-app.Tests/CMakeProjectCreatorTests.cs`: end-to-end validation of generated projects
- `test/fixtures/*`: golden projects used as expected output for generator tests

The generator creates a C++ CMake skeleton with `src/App`, `src/Static`, and `src/Dynamic`, plus optional `inc/` and `cmake/` folders. The tests compare generated output against fixtures and then run `cmake -S ... -B ...` and `cmake --build ...` for each generated project.

## Project-specific conventions

- Generated C++ code must keep 4-space indentation; do not compress multiline generator output into a single `AppendLine` call.
- Keep generator output formatting exact, because fixture tests compare file contents literally.
- If behavior changes, update the corresponding fixture files under `test/fixtures/*` to match the expected generated output.
- Test artifacts live under `temp/` with timestamped folders named `test-YYYY-MM-DD-HH-mm-ss`; avoid nested duplicate output folders and clean only stale `test-*` directories.
- The project version is declared in `src/create-cpp-app/create-cpp-app.csproj`; keep version metadata aligned with the CLI `--version` output.
- The build/test flow is `build.py` -> `python test.py` and should remain the primary validation path for this repo.
