using System.Text;

namespace crt_cpp_app;

public static class CMakeProjectCreator
{
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
                "-c", "user.name=crt-cpp-app",
                "-c", "user.email=crt-cpp-app@localhost",
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
        sb.EmplaceLine("#pragma once");
        sb.EmplaceLine();
        sb.EmplaceLine("#include <iostream>");
        sb.EmplaceLine();
        sb.EmplaceLine("inline void HelloWorld()");
        sb.EmplaceLine("{");
        sb.EmplaceLine("    std::cout << \"Hello World\" << std::endl;");
        sb.EmplaceLine("}");
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
        sb.EmplaceLine($"# {settings.ProjectName} cmake functions");
        sb.EmplaceLine();
        sb.EmplaceLine("# Add every child project below MY_SRC_DIR.");
        sb.EmplaceLine("# Usage: call add_projects() once from the root CMakeLists.txt after including this file.");
        sb.EmplaceLine("# Each child directory must contain CMakeLists.txt, define a target named after its directory, and be independent of scan order.");
        sb.EmplaceLine("# CONFIGURE_DEPENDS makes CMake rescan when projects are added or removed; rerun CMake if your generator does not support it.");
        sb.EmplaceLine("function(add_projects)");
        sb.EmplaceLine("    set(installable_projects)");
        sb.EmplaceLine("    file(GLOB_RECURSE cmake_lists RELATIVE \"${MY_SRC_DIR}\" CONFIGURE_DEPENDS \"${MY_SRC_DIR}/*/CMakeLists.txt\")");
        sb.EmplaceLine("    foreach(cmake_list IN LISTS cmake_lists)");
        sb.EmplaceLine("        get_filename_component(project_directory \"${cmake_list}\" DIRECTORY)");
        sb.EmplaceLine("        if(NOT project_directory STREQUAL \".\")");
        sb.EmplaceLine("            add_subdirectory(\"${MY_SRC_DIR}/${project_directory}\")");
        sb.EmplaceLine("            get_filename_component(project_name \"${project_directory}\" NAME)");
        sb.EmplaceLine("            get_filename_component(project_folder \"${project_directory}\" DIRECTORY)");
        sb.EmplaceLine("            if(TARGET \"${project_name}\")");
        sb.EmplaceLine("                set_property(TARGET \"${project_name}\" PROPERTY FOLDER \"${project_folder}\")");
        sb.EmplaceLine("                get_target_property(project_type \"${project_name}\" TYPE)");
        sb.EmplaceLine("                if(project_type STREQUAL \"EXECUTABLE\" OR project_type STREQUAL \"STATIC_LIBRARY\" OR project_type STREQUAL \"SHARED_LIBRARY\" OR project_type STREQUAL \"MODULE_LIBRARY\")");
        sb.EmplaceLine("                    list(APPEND installable_projects \"${project_name}\")");
        sb.EmplaceLine("                endif()");
        sb.EmplaceLine("            endif()");
        sb.EmplaceLine("        endif()");
        sb.EmplaceLine("    endforeach()");
        sb.EmplaceLine("    set_property(GLOBAL PROPERTY PROJECT_INSTALLABLE_TARGETS \"${installable_projects}\")");
        sb.EmplaceLine("endfunction()");
        sb.EmplaceLine();
        sb.EmplaceLine("# Install all executable and library targets discovered by add_projects.");
        sb.EmplaceLine("# Usage: call install_projects() after add_projects() in the root CMakeLists.txt.");
        sb.EmplaceLine("# Executables and shared libraries go to bin/; static and import libraries go to lib/ under CMAKE_INSTALL_PREFIX.");
        sb.EmplaceLine("function(install_projects)");
        sb.EmplaceLine("    get_property(installable_projects GLOBAL PROPERTY PROJECT_INSTALLABLE_TARGETS)");
        sb.EmplaceLine("    if(installable_projects)");
        sb.EmplaceLine("        install(TARGETS ${installable_projects}");
        sb.EmplaceLine("            RUNTIME DESTINATION bin");
        sb.EmplaceLine("            LIBRARY DESTINATION lib");
        sb.EmplaceLine("            ARCHIVE DESTINATION lib)");
        sb.EmplaceLine("    endif()");
        sb.EmplaceLine("endfunction()");
        sb.EmplaceLine();
        sb.EmplaceLine("# Collect C/C++ source and header files recursively.");
        sb.EmplaceLine("# Usage: search_project_files(<project-directory> <output-variable>).");
        sb.EmplaceLine("# The output variable is set in the caller's scope. CMake reconfigures when matching files change.");
        sb.EmplaceLine("function(search_project_files project_directory project_files)");
        sb.EmplaceLine("    file(GLOB_RECURSE files CONFIGURE_DEPENDS");
        sb.EmplaceLine("        \"${project_directory}/*.c\"");
        sb.EmplaceLine("        \"${project_directory}/*.cc\"");
        sb.EmplaceLine("        \"${project_directory}/*.cpp\"");
        sb.EmplaceLine("        \"${project_directory}/*.cxx\"");
        sb.EmplaceLine("        \"${project_directory}/*.h\"");
        sb.EmplaceLine("        \"${project_directory}/*.hh\"");
        sb.EmplaceLine("        \"${project_directory}/*.hpp\"");
        sb.EmplaceLine("        \"${project_directory}/*.hxx\")");
        sb.EmplaceLine("    set(${project_files} \"${files}\" PARENT_SCOPE)");
        sb.EmplaceLine("endfunction()");
        sb.EmplaceLine();
        sb.EmplaceLine("# Put files in IDE source groups that mirror their directories.");
        sb.EmplaceLine("# Usage: group_project_files(<project-directory> <file>...). This only affects IDE presentation.");
        sb.EmplaceLine("function(group_project_files project_directory)");
        sb.EmplaceLine("    foreach(project_file IN LISTS ARGN)");
        sb.EmplaceLine("        get_filename_component(project_file_directory \"${project_file}\" DIRECTORY)");
        sb.EmplaceLine("        file(RELATIVE_PATH project_filter \"${project_directory}\" \"${project_file_directory}\")");
        sb.EmplaceLine("        source_group(\"${project_filter}\" FILES \"${project_file}\")");
        sb.EmplaceLine("    endforeach()");
        sb.EmplaceLine("endfunction()");
        sb.EmplaceLine();
        sb.EmplaceLine("# Configure conventional Public/ and Private/ include directories for a target.");
        sb.EmplaceLine("# Usage: include_project_directories(<target> <project-directory>).");
        sb.EmplaceLine("# Headers in Public/ are exposed to consumers; Private/ is used only while compiling this target.");
        sb.EmplaceLine("function(include_project_directories target project_directory)");
        sb.EmplaceLine("    if(EXISTS \"${project_directory}/Public\")");
        sb.EmplaceLine("        target_include_directories(${target} PUBLIC \"${project_directory}/Public\")");
        sb.EmplaceLine("    endif()");
        sb.EmplaceLine("    if(EXISTS \"${project_directory}/Private\")");
        sb.EmplaceLine("        target_include_directories(${target} PRIVATE \"${project_directory}\" \"${project_directory}/Private\")");
        sb.EmplaceLine("    endif()");
        sb.EmplaceLine("endfunction()");
        sb.EmplaceLine();
        sb.EmplaceLine("# Define an executable from files in the current project directory.");
        sb.EmplaceLine("# Usage in a child CMakeLists.txt: project(MyApp) followed by define_executable().");
        sb.EmplaceLine("# Call link_internal_projects(...) afterwards to link targets defined elsewhere in this solution.");
        sb.EmplaceLine("function(define_executable)");
        sb.EmplaceLine("    search_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" project_files)");
        sb.EmplaceLine("    add_executable(${PROJECT_NAME} ${project_files})");
        sb.EmplaceLine("    group_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" ${project_files})");
        sb.EmplaceLine("    include_project_directories(${PROJECT_NAME} \"${CMAKE_CURRENT_SOURCE_DIR}\")");
        sb.EmplaceLine("    target_include_directories(${PROJECT_NAME} PRIVATE ${MY_INC_DIR})");
        sb.EmplaceLine("endfunction()");
        sb.EmplaceLine();
        sb.EmplaceLine("# Define a static library from files in the current project directory.");
        sb.EmplaceLine("# Usage in a child CMakeLists.txt: project(MyLibrary) followed by define_static_library().");
        sb.EmplaceLine("# Public/ headers become part of the library's public include interface.");
        sb.EmplaceLine("function(define_static_library)");
        sb.EmplaceLine("    search_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" project_files)");
        sb.EmplaceLine("    add_library(${PROJECT_NAME} STATIC ${project_files})");
        sb.EmplaceLine("    group_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" ${project_files})");
        sb.EmplaceLine("    include_project_directories(${PROJECT_NAME} \"${CMAKE_CURRENT_SOURCE_DIR}\")");
        sb.EmplaceLine("    target_include_directories(${PROJECT_NAME} PRIVATE ${MY_INC_DIR})");
        sb.EmplaceLine("endfunction()");
        sb.EmplaceLine();
        sb.EmplaceLine("# Define a shared library from files in the current project directory.");
        sb.EmplaceLine("# Usage in a child CMakeLists.txt: project(MyLibrary) followed by define_shared_library().");
        sb.EmplaceLine("# Exported symbols still need platform-appropriate export macros in public headers.");
        sb.EmplaceLine("function(define_shared_library)");
        sb.EmplaceLine("    search_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" project_files)");
        sb.EmplaceLine("    add_library(${PROJECT_NAME} SHARED ${project_files})");
        sb.EmplaceLine("    group_project_files(\"${CMAKE_CURRENT_SOURCE_DIR}\" ${project_files})");
        sb.EmplaceLine("    include_project_directories(${PROJECT_NAME} \"${CMAKE_CURRENT_SOURCE_DIR}\")");
        sb.EmplaceLine("    target_include_directories(${PROJECT_NAME} PRIVATE ${MY_INC_DIR})");
        sb.EmplaceLine("endfunction()");
        sb.EmplaceLine();
        sb.EmplaceLine("# Link targets from this CMake solution to the current project.");
        sb.EmplaceLine("# Usage: link_internal_projects(TargetA TargetB ...). Targets are linked with PUBLIC visibility.");
        sb.EmplaceLine("# Use target_link_libraries directly when PRIVATE or INTERFACE visibility is required.");
        sb.EmplaceLine("function(link_internal_projects)");
        sb.EmplaceLine("    target_link_libraries(${PROJECT_NAME} PUBLIC ${ARGN})");
        sb.EmplaceLine("endfunction()");
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
        sb.EmplaceLine("#pragma once");
        sb.EmplaceLine();
        sb.EmplaceLine("void HelloStatic();");

        var path = Path.Combine(settings.StaticDir, "Public", "StaticLib.h");
        WriteFile(path, sb.ToString());
    }

    private static void CreateStaticSourceFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.EmplaceLine("#include \"StaticLib.h\"");
        sb.EmplaceLine("#include <iostream>");
        sb.EmplaceLine();
        sb.EmplaceLine("void HelloStatic()");
        sb.EmplaceLine("{");
        sb.EmplaceLine("    std::cout << \"Hello Static Library\" << std::endl;");
        sb.EmplaceLine("}");

        var path = Path.Combine(settings.StaticDir, "Private", "StaticLib.cpp");
        WriteFile(path, sb.ToString());
    }

    private static void CreateStaticCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.EmplaceLine("project(Static)");
        sb.EmplaceLine("define_static_library()");

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
        sb.EmplaceLine("#pragma once");
        sb.EmplaceLine();
        sb.EmplaceLine("#if defined(_WIN32) || defined(__CYGWIN__)");
        sb.EmplaceLine("    #ifdef Dynamic_EXPORTS");
        sb.EmplaceLine("        #define DYNAMIC_API __declspec(dllexport)");
        sb.EmplaceLine("    #else");
        sb.EmplaceLine("        #define DYNAMIC_API __declspec(dllimport)");
        sb.EmplaceLine("    #endif");
        sb.EmplaceLine("#elif defined(__GNUC__) && __GNUC__ >= 4");
        sb.EmplaceLine("    #define DYNAMIC_API __attribute__ ((visibility (\"default\")))");
        sb.EmplaceLine("#else");
        sb.EmplaceLine("    #define DYNAMIC_API");
        sb.EmplaceLine("#endif");

        var path = Path.Combine(settings.DynamicDir, "Public", "DynamicExports.h");
        WriteFile(path, sb.ToString());
    }

    private static void CreateDynamicHeaderFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.EmplaceLine("#pragma once");
        sb.EmplaceLine();
        sb.EmplaceLine("#include \"DynamicExports.h\"");
        sb.EmplaceLine();
        sb.EmplaceLine("DYNAMIC_API void HelloDynamic();");

        var path = Path.Combine(settings.DynamicDir, "Public", "DynamicLib.h");
        WriteFile(path, sb.ToString());
    }

    private static void CreateDynamicSourceFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.EmplaceLine("#include \"DynamicLib.h\"");
        sb.EmplaceLine();
        sb.EmplaceLine("#include <iostream>");
        sb.EmplaceLine();
        sb.EmplaceLine("void HelloDynamic()");
        sb.EmplaceLine("{");
        sb.EmplaceLine("    std::cout << \"Hello Dynamic Library\" << std::endl;");
        sb.EmplaceLine("}");

        var path = Path.Combine(settings.DynamicDir, "Private", "DynamicLib.cpp");
        WriteFile(path, sb.ToString());
    }

    private static void CreateDynamicCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.EmplaceLine("project(Dynamic)");
        sb.EmplaceLine("define_shared_library()");

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
        sb.EmplaceLine("project(App)");
        sb.EmplaceLine("define_executable()");
        sb.EmplaceLine("link_internal_projects(Static Dynamic)");

        var path = Path.Combine(settings.AppDir, "CMakeLists.txt");
        WriteFile(path, sb.ToString());
    }

    private static void CreateAppMainFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.EmplaceLine("#include <iostream>");
        sb.EmplaceLine($"#include \"{settings.ProjectName}.h\"");
        sb.EmplaceLine("#include \"StaticLib.h\"");
        sb.EmplaceLine("#include \"DynamicLib.h\"");
        sb.EmplaceLine();
        sb.EmplaceLine("int main()");
        sb.EmplaceLine("{");
        sb.EmplaceLine("    HelloWorld();");
        sb.EmplaceLine("    HelloStatic();");
        sb.EmplaceLine("    HelloDynamic();");
        sb.EmplaceLine("    return 0;");
        sb.EmplaceLine("}");

        WriteFile(Path.Combine(settings.AppDir, "main.cpp"), sb.ToString());
    }

    private static void CreateSolutionCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.EmplaceLine("cmake_minimum_required(VERSION 3.20)");
        sb.EmplaceLine($"project({settings.ProjectName} LANGUAGES CXX)");
        sb.EmplaceLine();
        sb.EmplaceLine($"set(CMAKE_CXX_STANDARD {settings.CppStandard})");
        sb.EmplaceLine("set(CMAKE_CXX_STANDARD_REQUIRED ON)");
        sb.EmplaceLine();
        sb.EmplaceLine(@"set(MY_REPO_DIR ""${CMAKE_CURRENT_SOURCE_DIR}"")");
        sb.EmplaceLine(@"set(MY_SRC_DIR ""${MY_REPO_DIR}/src"")");
        sb.EmplaceLine(@"set(MY_BINARY_DIR ""${MY_REPO_DIR}/bin"")");
        sb.EmplaceLine(@"set(MY_BUILD_DIR ""${MY_REPO_DIR}/build"")");
        sb.EmplaceLine(@"set(MY_CMAKE_DIR ""${MY_REPO_DIR}/cmake"")");

        sb.EmplaceLine(@"set(MY_INC_DIR ""${MY_REPO_DIR}/inc"")");
        sb.EmplaceLine(@"set(MY_RES_DIR ""${MY_REPO_DIR}/res"")");
        sb.EmplaceLine(@"set(MY_3RD_DIR ""${MY_REPO_DIR}/3rd"")");
        sb.EmplaceLine(@"set(MY_PATCH_DIR ""${MY_REPO_DIR}/patch"")");

        sb.EmplaceLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        sb.EmplaceLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        sb.EmplaceLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        sb.EmplaceLine();
        sb.EmplaceLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        sb.EmplaceLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        sb.EmplaceLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        sb.EmplaceLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        sb.EmplaceLine();
        sb.EmplaceLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        sb.EmplaceLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        sb.EmplaceLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        sb.EmplaceLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        sb.EmplaceLine();
        sb.EmplaceLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        sb.EmplaceLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        sb.EmplaceLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        sb.EmplaceLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        sb.EmplaceLine();
        sb.EmplaceLine($"include(${{MY_CMAKE_DIR}}/{settings.ProjectName}.cmake)");
        sb.EmplaceLine();

        sb.EmplaceLine("add_projects()");
        sb.EmplaceLine("install_projects()");

        WriteFile(Path.Combine(settings.ProjectDir, "CMakeLists.txt"), sb.ToString());
    }
}
