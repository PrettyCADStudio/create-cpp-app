using System.Text;

namespace create_cpp_app;

public static class CMakeProjectCreator
{
    private const string NewLine = "\n";

    private static void AppendLine(StringBuilder sb, string line, string newline = NewLine)
    {
        sb.Append(line);
        sb.Append(newline);
    }

    private static void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public static void Create(CMakeProjectSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();

        CreateIncFolder(settings);
        CreateResFolder(settings);
        CreateThirdPartyFolder(settings);
        CreatePatchFolder(settings);
        CreateCMakeFolder(settings);
        CreateDevelopmentScripts(settings);
        CreateStaticProject(settings);
        CreateDynamicProject(settings);
        CreateAppProject(settings);
        CreateSolutionCMakeFile(settings);
        CreateGitRepository(settings);
    }

    private static void CreateGitRepository(CMakeProjectSettings settings)
    {
        if (!settings.InitializeGit || !IsGitAvailable())
        {
            return;
        }

        CreateGitIgnore(settings);

        if (!RunGit(settings.ProjectDir, "init"))
        {
            Console.Error.WriteLine("Warning: Git repository initialization was not completed.");
            return;
        }

        CreatePipenvLockFile(settings);

        if (!RunGit(settings.ProjectDir, "add", "--all") ||
            !RunGit(
                settings.ProjectDir,
                "-c", "user.name=create-cpp-app",
                "-c", "user.email=create-cpp-app@localhost",
                "commit", "-m", "Initial project"))
        {
            Console.Error.WriteLine("Warning: Git repository initialization was not completed.");
            return;
        }

        Console.WriteLine("  Git repository initialized with an initial commit.");
    }

    private static void CreatePipenvLockFile(CMakeProjectSettings settings)
    {
        if (settings.PythonScripts != PythonScriptMode.Pipenv || !IsPipenvAvailable())
        {
            return;
        }

        if (!RunProcess("pipenv", settings.ProjectDir, "install"))
        {
            Console.Error.WriteLine("Warning: Pipenv could not create Pipfile.lock; continuing without it.");
        }
    }

    private static void CreateGitIgnore(CMakeProjectSettings settings)
    {
        WriteFile(Path.Combine(settings.ProjectDir, ".gitignore"), """
            # CMake build output
            /build/
            /bin/
            /install/
            /dist/
            CMakeCache.txt
            CMakeFiles/
            cmake_install.cmake
            Makefile
            compile_commands.json

            # IDE files
            /.vs/
            /.idea/
            *.sln
            *.vcxproj
            *.vcxproj.filters
            *.vcxproj.user

            # Python development files
            __pycache__/
            *.py[cod]
            .venv/

            # Operating system files
            .DS_Store
            Thumbs.db
            """);
    }

    private static bool IsGitAvailable()
    {
        return RunProcess("git", Directory.GetCurrentDirectory(), "--version");
    }

    private static bool IsPipenvAvailable()
    {
        return RunProcess("pipenv", Directory.GetCurrentDirectory(), "--version");
    }

    private static bool RunGit(string workingDirectory, params string[] arguments)
    {
        return RunProcess("git", workingDirectory, arguments);
    }

    private static bool RunProcess(string fileName, string workingDirectory, params string[] arguments)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process is null)
            {
                return false;
            }

            process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return false;
        }
    }

    private static void CreateIncFolder(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.IncDir);

        var sb = new StringBuilder();
        AppendLine(sb, "#pragma once");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "#include <iostream>");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "inline void HelloWorld()");
        AppendLine(sb, "{");
        AppendLine(sb, "    std::cout << \"Hello World\" << std::endl;");
        AppendLine(sb, "}");
        WriteFile(Path.Combine(settings.IncDir, $"{settings.ProjectName}.h"), sb.ToString());
    }

    private static void CreateResFolder(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.ResDir);
        WriteFile(Path.Combine(settings.ResDir, ".keep"), string.Empty);
    }

    private static void CreateThirdPartyFolder(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.ThirdPartyDir);
        WriteFile(Path.Combine(settings.ThirdPartyDir, ".keep"), string.Empty);
    }

    private static void CreatePatchFolder(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.PatchDir);
        WriteFile(Path.Combine(settings.PatchDir, ".keep"), string.Empty);
    }

    private static void CreateCMakeFolder(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.CmakeDir);

        var sb = new StringBuilder();
        AppendLine(sb, $"# {settings.ProjectName} cmake functions");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Add every child project below MY_SRC_DIR.");
        AppendLine(sb, "# Usage: call add_projects() once from the root CMakeLists.txt after including this file.");
        AppendLine(sb, "# Each child directory must contain CMakeLists.txt, define a target named after its directory, and be independent of scan order.");
        AppendLine(sb, "# CONFIGURE_DEPENDS makes CMake rescan when projects are added or removed; rerun CMake if your generator does not support it.");
        AppendLine(sb, "function(add_projects)");
        AppendLine(sb, "    set(installable_projects)");
        AppendLine(sb, "    file(GLOB_RECURSE cmake_lists RELATIVE \"${MY_SRC_DIR}\" CONFIGURE_DEPENDS \"${MY_SRC_DIR}/*/CMakeLists.txt\")");
        AppendLine(sb, "    foreach(cmake_list IN LISTS cmake_lists)");
        AppendLine(sb, "        get_filename_component(project_directory \"${cmake_list}\" DIRECTORY)");
        AppendLine(sb, "        if(NOT project_directory STREQUAL \".\")");
        AppendLine(sb, "            add_subdirectory(\"${MY_SRC_DIR}/${project_directory}\")");
        AppendLine(sb, "            get_filename_component(project_name \"${project_directory}\" NAME)");
        AppendLine(sb, "            get_filename_component(project_folder \"${project_directory}\" DIRECTORY)");
        AppendLine(sb, "            if(TARGET \"${project_name}\")");
        AppendLine(sb, "                set_property(TARGET \"${project_name}\" PROPERTY FOLDER \"${project_folder}\")");
        AppendLine(sb, "                get_target_property(project_type \"${project_name}\" TYPE)");
        AppendLine(sb, "                if(project_type STREQUAL \"EXECUTABLE\" OR project_type STREQUAL \"STATIC_LIBRARY\" OR project_type STREQUAL \"SHARED_LIBRARY\" OR project_type STREQUAL \"MODULE_LIBRARY\")");
        AppendLine(sb, "                    list(APPEND installable_projects \"${project_name}\")");
        AppendLine(sb, "                endif()");
        AppendLine(sb, "            endif()");
        AppendLine(sb, "        endif()");
        AppendLine(sb, "    endforeach()");
        AppendLine(sb, "    set_property(GLOBAL PROPERTY PROJECT_INSTALLABLE_TARGETS \"${installable_projects}\")");
        AppendLine(sb, "endfunction()");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Install all executable and library targets discovered by add_projects.");
        AppendLine(sb, "# Usage: call install_projects() after add_projects() in the root CMakeLists.txt.");
        AppendLine(sb, "# Executables and shared libraries go to bin/; static and import libraries go to lib/ under CMAKE_INSTALL_PREFIX.");
        AppendLine(sb, "function(install_projects)");
        AppendLine(sb, "    get_property(installable_projects GLOBAL PROPERTY PROJECT_INSTALLABLE_TARGETS)");
        AppendLine(sb, "    if(installable_projects)");
        AppendLine(sb, "        install(TARGETS ${installable_projects}");
        AppendLine(sb, "            RUNTIME DESTINATION bin");
        AppendLine(sb, "            LIBRARY DESTINATION lib");
        AppendLine(sb, "            ARCHIVE DESTINATION lib)");
        AppendLine(sb, "    endif()");
        AppendLine(sb, "endfunction()");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Collect C/C++ source and header files recursively.");
        AppendLine(sb, "# Usage: search_project_files(<project-directory> <output-variable>).");
        AppendLine(sb, "# The output variable is set in the caller's scope. CMake reconfigures when matching files change.");
        AppendLine(sb, "function(search_project_files project_directory project_files)");
        AppendLine(sb, "    file(GLOB_RECURSE files CONFIGURE_DEPENDS");
        AppendLine(sb, "        \"${project_directory}/*.c\"");
        AppendLine(sb, "        \"${project_directory}/*.cc\"");
        AppendLine(sb, "        \"${project_directory}/*.cpp\"");
        AppendLine(sb, "        \"${project_directory}/*.cxx\"");
        AppendLine(sb, "        \"${project_directory}/*.h\"");
        AppendLine(sb, "        \"${project_directory}/*.hh\"");
        AppendLine(sb, "        \"${project_directory}/*.hpp\"");
        AppendLine(sb, "        \"${project_directory}/*.hxx\")");
        AppendLine(sb, "    set(${project_files} \"${files}\" PARENT_SCOPE)");
        AppendLine(sb, "endfunction()");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Put files in IDE source groups that mirror their directories.");
        AppendLine(sb, "# Usage: group_project_files(<project-directory> <file>...). This only affects IDE presentation.");
        AppendLine(sb, "function(group_project_files project_directory)");
        AppendLine(sb, "    foreach(project_file IN LISTS ARGN)");
        AppendLine(sb, "        get_filename_component(project_file_directory \"${project_file}\" DIRECTORY)");
        AppendLine(sb, "        file(RELATIVE_PATH project_filter \"${project_directory}\" \"${project_file_directory}\")");
        AppendLine(sb, "        source_group(\"${project_filter}\" FILES \"${project_file}\")");
        AppendLine(sb, "    endforeach()");
        AppendLine(sb, "endfunction()");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Configure conventional Public/ and Private/ include directories for a target.");
        AppendLine(sb, "# Usage: include_project_directories(<target> <project-directory>).");
        AppendLine(sb, "# Headers in Public/ are exposed to consumers; Private/ is used only while compiling this target.");
        AppendLine(sb, "function(include_project_directories target project_directory)");
        AppendLine(sb, "    if(EXISTS \"${project_directory}/Public\")");
        AppendLine(sb, "        target_include_directories(${target} PUBLIC \"${project_directory}/Public\")");
        AppendLine(sb, "    endif()");
        AppendLine(sb, "    if(EXISTS \"${project_directory}/Private\")");
        AppendLine(sb, "        target_include_directories(${target} PRIVATE \"${project_directory}\" \"${project_directory}/Private\")");
        AppendLine(sb, "    endif()");
        AppendLine(sb, "endfunction()");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Define an executable from files in the current project directory.");
        AppendLine(sb, "# Usage in a child CMakeLists.txt: project(MyApp) followed by define_executable().");
        AppendLine(sb, "# Call link_internal_projects(...) afterwards to link targets defined elsewhere in this solution.");
        AppendLine(sb, "function(define_executable)");
        AppendLine(sb, "    search_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" project_files)");
        AppendLine(sb, "    add_executable(${PROJECT_NAME} ${project_files})");
        AppendLine(sb, "    group_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" ${project_files})");
        AppendLine(sb, "    include_project_directories(${PROJECT_NAME} \"${CMAKE_CURRENT_SOURCE_DIR}\")");
        AppendLine(sb, "endfunction()");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Define a static library from files in the current project directory.");
        AppendLine(sb, "# Usage in a child CMakeLists.txt: project(MyLibrary) followed by define_static_library().");
        AppendLine(sb, "# Public/ headers become part of the library's public include interface.");
        AppendLine(sb, "function(define_static_library)");
        AppendLine(sb, "    search_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" project_files)");
        AppendLine(sb, "    add_library(${PROJECT_NAME} STATIC ${project_files})");
        AppendLine(sb, "    group_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" ${project_files})");
        AppendLine(sb, "    include_project_directories(${PROJECT_NAME} \"${CMAKE_CURRENT_SOURCE_DIR}\")");
        AppendLine(sb, "endfunction()");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Define a shared library from files in the current project directory.");
        AppendLine(sb, "# Usage in a child CMakeLists.txt: project(MyLibrary) followed by define_shared_library().");
        AppendLine(sb, "# Exported symbols still need platform-appropriate export macros in public headers.");
        AppendLine(sb, "function(define_shared_library)");
        AppendLine(sb, "    search_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" project_files)");
        AppendLine(sb, "    add_library(${PROJECT_NAME} SHARED ${project_files})");
        AppendLine(sb, "    group_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" ${project_files})");
        AppendLine(sb, "    include_project_directories(${PROJECT_NAME} \"${CMAKE_CURRENT_SOURCE_DIR}\")");
        AppendLine(sb, "endfunction()");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "# Link targets from this CMake solution to the current project.");
        AppendLine(sb, "# Usage: link_internal_projects(TargetA TargetB ...). Targets are linked with PUBLIC visibility.");
        AppendLine(sb, "# Use target_link_libraries directly when PRIVATE or INTERFACE visibility is required.");
        AppendLine(sb, "function(link_internal_projects)");
        AppendLine(sb, "    target_link_libraries(${PROJECT_NAME} PUBLIC ${ARGN})");
        AppendLine(sb, "endfunction()");
        WriteFile(Path.Combine(settings.CmakeDir, $"{settings.ProjectName}.cmake"), sb.ToString());
    }

    private static void CreateDevelopmentScripts(CMakeProjectSettings settings)
    {
        if (settings.PythonScripts == PythonScriptMode.None)
        {
            return;
        }

        var scriptDir = settings.PythonScripts == PythonScriptMode.Pipenv
            ? Path.Combine(settings.ProjectDir, "scripts")
            : settings.ProjectDir;
        Directory.CreateDirectory(scriptDir);

        WriteFile(Path.Combine(scriptDir, "mksln.py"), """
            import argparse
            import subprocess
            from pathlib import Path

            SCRIPT_DIR = Path(__file__).resolve().parent
            PROJECT_DIR = SCRIPT_DIR.parent if SCRIPT_DIR.name == "scripts" else SCRIPT_DIR

            def main():
                parser = argparse.ArgumentParser(description="Configure the CMake project and generate build files")
                parser.add_argument("-G", "--generator", help="CMake generator, for example Ninja or Visual Studio 18 2026")
                args = parser.parse_args()

                command = ["cmake", "-S", str(PROJECT_DIR), "-B", str(PROJECT_DIR / "build")]
                if args.generator:
                    command.extend(["-G", args.generator])
                subprocess.run(command, check=True)

            if __name__ == "__main__":
                main()
            """);
        WriteFile(Path.Combine(scriptDir, "build.py"), """
            import argparse
            import subprocess
            from pathlib import Path

            SCRIPT_DIR = Path(__file__).resolve().parent
            PROJECT_DIR = SCRIPT_DIR.parent if SCRIPT_DIR.name == "scripts" else SCRIPT_DIR
            BUILD_DIR = PROJECT_DIR / "build"

            def main():
                parser = argparse.ArgumentParser(description="Configure and build the CMake project")
                parser.add_argument("--config", default="Release", help="Build configuration (default: Release)")
                args = parser.parse_args()

                subprocess.run(["cmake", "-S", str(PROJECT_DIR), "-B", str(BUILD_DIR)], check=True)
                subprocess.run(["cmake", "--build", str(BUILD_DIR), "--config", args.config], check=True)

            if __name__ == "__main__":
                main()
            """);
        WriteFile(Path.Combine(scriptDir, "install.py"), """
            import argparse
            import subprocess
            from pathlib import Path

            SCRIPT_DIR = Path(__file__).resolve().parent
            PROJECT_DIR = SCRIPT_DIR.parent if SCRIPT_DIR.name == "scripts" else SCRIPT_DIR
            BUILD_DIR = PROJECT_DIR / "build"

            def main():
                parser = argparse.ArgumentParser(description="Install the CMake project")
                parser.add_argument("--config", default="Release", help="Build configuration (default: Release)")
                parser.add_argument("--prefix", default=str(PROJECT_DIR / "install"), help="Install destination (default: <project>/install)")
                args = parser.parse_args()

                subprocess.run(["cmake", "--install", str(BUILD_DIR), "--config", args.config, "--prefix", args.prefix], check=True)

            if __name__ == "__main__":
                main()
            """);
        WriteFile(Path.Combine(scriptDir, "build-install.py"), """
            import argparse
            import subprocess
            import sys
            from pathlib import Path

            SCRIPT_DIR = Path(__file__).resolve().parent

            def main():
                parser = argparse.ArgumentParser(description="Build and install the CMake project")
                parser.add_argument("--config", default="Release", help="Build configuration (default: Release)")
                parser.add_argument("--prefix", help="Install destination (default: <project>/install)")
                args = parser.parse_args()

                subprocess.run([sys.executable, str(SCRIPT_DIR / "build.py"), "--config", args.config], check=True)
                command = [sys.executable, str(SCRIPT_DIR / "install.py"), "--config", args.config]
                if args.prefix:
                    command.extend(["--prefix", args.prefix])
                subprocess.run(command, check=True)

            if __name__ == "__main__":
                main()
            """);
        WriteFile(Path.Combine(scriptDir, "archive.py"), """
            import argparse
            import shutil
            import subprocess
            import sys
            from pathlib import Path

            SCRIPT_DIR = Path(__file__).resolve().parent
            PROJECT_DIR = SCRIPT_DIR.parent if SCRIPT_DIR.name == "scripts" else SCRIPT_DIR

            def main():
                parser = argparse.ArgumentParser(description="Build and archive the CMake project output")
                parser.add_argument("--config", default="Release", help="Build configuration (default: Release)")
                args = parser.parse_args()

                subprocess.run([sys.executable, str(SCRIPT_DIR / "build.py"), "--config", args.config], check=True)
                output_dir = PROJECT_DIR / "bin"
                if not output_dir.is_dir():
                    raise RuntimeError(f"Build output directory not found: {output_dir}")
                dist_dir = PROJECT_DIR / "dist"
                dist_dir.mkdir(exist_ok=True)
                archive = shutil.make_archive(str(dist_dir / f"{PROJECT_DIR.name}-{args.config}"), "zip", output_dir)
                print(archive)

            if __name__ == "__main__":
                main()
            """);

        if (settings.PythonScripts == PythonScriptMode.Pipenv)
        {
            WriteFile(Path.Combine(settings.ProjectDir, "Pipfile"), """
                [[source]]
                url = "https://pypi.org/simple"
                verify_ssl = true
                name = "pypi"

                [requires]
                python_version = "3"

                [scripts]
                mksln = "python scripts/mksln.py"
                build = "python scripts/build.py"
                install = "python scripts/install.py"
                build-install = "python scripts/build-install.py"
                archive = "python scripts/archive.py"
                """);
        }
    }

    private static void CreateStaticProject(CMakeProjectSettings settings)
    {
        var publicDir = Path.Combine(settings.StaticDir, "Public");
        var privateDir = Path.Combine(settings.StaticDir, "Private");

        Directory.CreateDirectory(settings.StaticDir);
        Directory.CreateDirectory(publicDir);
        Directory.CreateDirectory(privateDir);

        CreateStaticHeaderFile(settings);
        CreateStaticSourceFile(settings);
        CreateStaticCMakeFile(settings);
    }

    private static void CreateStaticHeaderFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "#pragma once");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "void HelloStatic();");

        var path = Path.Combine(settings.StaticDir, "Public", "StaticLib.h");
        WriteFile(path, sb.ToString());
    }

    private static void CreateStaticSourceFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "#include \"StaticLib.h\"");
        AppendLine(sb, "#include <iostream>");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "void HelloStatic()");
        AppendLine(sb, "{");
        AppendLine(sb, "    std::cout << \"Hello Static Library\" << std::endl;");
        AppendLine(sb, "}");

        var path = Path.Combine(settings.StaticDir, "Private", "StaticLib.cpp");
        WriteFile(path, sb.ToString());
    }

    private static void CreateStaticCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "project(Static)");
        AppendLine(sb, "define_static_library()");

        var path = Path.Combine(settings.StaticDir, "CMakeLists.txt");
        WriteFile(path, sb.ToString());
    }

    private static void CreateDynamicProject(CMakeProjectSettings settings)
    {
        var publicDir = Path.Combine(settings.DynamicDir, "Public");
        var privateDir = Path.Combine(settings.DynamicDir, "Private");

        Directory.CreateDirectory(settings.DynamicDir);
        Directory.CreateDirectory(publicDir);
        Directory.CreateDirectory(privateDir);

        CreateDynamicExportFile(settings);
        CreateDynamicHeaderFile(settings);
        CreateDynamicSourceFile(settings);
        CreateDynamicCMakeFile(settings);
    }

    private static void CreateDynamicExportFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "#pragma once");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "#if defined(_WIN32) || defined(__CYGWIN__)");
        AppendLine(sb, "    #ifdef Dynamic_EXPORTS");
        AppendLine(sb, "        #define DYNAMIC_API __declspec(dllexport)");
        AppendLine(sb, "    #else");
        AppendLine(sb, "        #define DYNAMIC_API __declspec(dllimport)");
        AppendLine(sb, "    #endif");
        AppendLine(sb, "#elif defined(__GNUC__) && __GNUC__ >= 4");
        AppendLine(sb, "    #define DYNAMIC_API __attribute__ ((visibility (\"default\")))");
        AppendLine(sb, "#else");
        AppendLine(sb, "    #define DYNAMIC_API");
        AppendLine(sb, "#endif");

        var path = Path.Combine(settings.DynamicDir, "Public", "DynamicExports.h");
        WriteFile(path, sb.ToString());
    }

    private static void CreateDynamicHeaderFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "#pragma once");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "#include \"DynamicExports.h\"");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "DYNAMIC_API void HelloDynamic();");

        var path = Path.Combine(settings.DynamicDir, "Public", "DynamicLib.h");
        WriteFile(path, sb.ToString());
    }

    private static void CreateDynamicSourceFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "#include \"DynamicLib.h\"");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "#include <iostream>");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "void HelloDynamic()");
        AppendLine(sb, "{");
        AppendLine(sb, "    std::cout << \"Hello Dynamic Library\" << std::endl;");
        AppendLine(sb, "}");

        var path = Path.Combine(settings.DynamicDir, "Private", "DynamicLib.cpp");
        WriteFile(path, sb.ToString());
    }

    private static void CreateDynamicCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "project(Dynamic)");
        AppendLine(sb, "define_shared_library()");

        var path = Path.Combine(settings.DynamicDir, "CMakeLists.txt");
        WriteFile(path, sb.ToString());
    }

    private static void CreateAppProject(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.AppDir);
        CreateAppCMakeFile(settings);
        CreateAppMainFile(settings);
    }

    private static void CreateAppCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "project(App)");
        AppendLine(sb, "define_executable()");
        AppendLine(sb, "target_include_directories(App PRIVATE ${MY_INC_DIR})");
        AppendLine(sb, "link_internal_projects(Static Dynamic)");

        var path = Path.Combine(settings.AppDir, "CMakeLists.txt");
        WriteFile(path, sb.ToString());
    }

    private static void CreateAppMainFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "#include <iostream>");
        AppendLine(sb, $"#include \"{settings.ProjectName}.h\"");
        AppendLine(sb, "#include \"StaticLib.h\"");
        AppendLine(sb, "#include \"DynamicLib.h\"");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "int main()");
        AppendLine(sb, "{");
        AppendLine(sb, "    HelloWorld();");
        AppendLine(sb, "    HelloStatic();");
        AppendLine(sb, "    HelloDynamic();");
        AppendLine(sb, "    return 0;");
        AppendLine(sb, "}");

        WriteFile(Path.Combine(settings.AppDir, "main.cpp"), sb.ToString());
    }

    private static void CreateSolutionCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "cmake_minimum_required(VERSION 3.20)");
        AppendLine(sb, $"project({settings.ProjectName} LANGUAGES CXX)");
        AppendLine(sb, string.Empty);
        AppendLine(sb, $"set(CMAKE_CXX_STANDARD {settings.CppStandard})");
        AppendLine(sb, "set(CMAKE_CXX_STANDARD_REQUIRED ON)");
        AppendLine(sb, string.Empty);
        AppendLine(sb, @"set(MY_REPO_DIR ""${CMAKE_CURRENT_SOURCE_DIR}"")");
        AppendLine(sb, @"set(MY_SRC_DIR ""${MY_REPO_DIR}/src"")");
        AppendLine(sb, @"set(MY_BINARY_DIR ""${MY_REPO_DIR}/bin"")");
        AppendLine(sb, @"set(MY_BUILD_DIR ""${MY_REPO_DIR}/build"")");
        AppendLine(sb, @"set(MY_CMAKE_DIR ""${MY_REPO_DIR}/cmake"")");

        AppendLine(sb, @"set(MY_INC_DIR ""${MY_REPO_DIR}/inc"")");
        AppendLine(sb, @"set(MY_RES_DIR ""${MY_REPO_DIR}/res"")");
        AppendLine(sb, @"set(MY_3RD_DIR ""${MY_REPO_DIR}/3rd"")");
        AppendLine(sb, @"set(MY_PATCH_DIR ""${MY_REPO_DIR}/patch"")");

        AppendLine(sb, @"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        AppendLine(sb, @"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        AppendLine(sb, @"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        AppendLine(sb, string.Empty);
        AppendLine(sb, @"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        AppendLine(sb, @"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        AppendLine(sb, @"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        AppendLine(sb, @"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        AppendLine(sb, string.Empty);
        AppendLine(sb, @"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        AppendLine(sb, @"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        AppendLine(sb, @"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        AppendLine(sb, @"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        AppendLine(sb, string.Empty);
        AppendLine(sb, @"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        AppendLine(sb, @"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        AppendLine(sb, @"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        AppendLine(sb, @"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        AppendLine(sb, string.Empty);
        AppendLine(sb, $"include(${{MY_CMAKE_DIR}}/{settings.ProjectName}.cmake)");
        AppendLine(sb, string.Empty);

        AppendLine(sb, "add_projects()");
        AppendLine(sb, "install_projects()");

        WriteFile(Path.Combine(settings.ProjectDir, "CMakeLists.txt"), sb.ToString());
    }
}
