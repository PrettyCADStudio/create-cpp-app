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
        CreateStaticProject(settings);
        CreateDynamicProject(settings);
        CreateAppProject(settings);
        CreateSolutionCMakeFile(settings);
    }

    private static void CreateIncFolder(CMakeProjectSettings settings)
    {
        if (settings.UseIncFolder)
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
    }

    private static void CreateResFolder(CMakeProjectSettings settings)
    {
        if (!settings.UseResFolder)
        {
            return;
        }

        Directory.CreateDirectory(settings.ResDir);
        WriteFile(Path.Combine(settings.ResDir, ".keep"), string.Empty);
    }

    private static void CreateThirdPartyFolder(CMakeProjectSettings settings)
    {
        if (!settings.UseThirdPartyFolder)
        {
            return;
        }

        Directory.CreateDirectory(settings.ThirdPartyDir);
        WriteFile(Path.Combine(settings.ThirdPartyDir, ".keep"), string.Empty);
    }

    private static void CreatePatchFolder(CMakeProjectSettings settings)
    {
        if (!settings.UsePatchFolder)
        {
            return;
        }

        Directory.CreateDirectory(settings.PatchDir);
        WriteFile(Path.Combine(settings.PatchDir, ".keep"), string.Empty);
    }

    private static void CreateCMakeFolder(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.CmakeDir);

        var sb = new StringBuilder();
        AppendLine(sb, $"# {settings.ProjectName} cmake functions");
        WriteFile(Path.Combine(settings.CmakeDir, $"{settings.ProjectName}.cmake"), sb.ToString());
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
        AppendLine(sb, "add_library(Static STATIC Private/StaticLib.cpp)");
        AppendLine(sb, "target_include_directories(Static PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/Public)");

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
        AppendLine(sb, "add_library(Dynamic SHARED Private/DynamicLib.cpp)");
        AppendLine(sb, "target_include_directories(Dynamic PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/Public)");

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
        AppendLine(sb, "add_executable(App main.cpp)");
        if (settings.UseIncFolder)
        {
            AppendLine(sb, "include_directories(${MY_INC_DIR})");
        }
        AppendLine(sb, "target_link_libraries(App PRIVATE Static)");
        AppendLine(sb, "target_link_libraries(App PRIVATE Dynamic)");

        var path = Path.Combine(settings.AppDir, "CMakeLists.txt");
        WriteFile(path, sb.ToString());
    }

    private static void CreateAppMainFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "#include <iostream>");
        if (settings.UseIncFolder)
        {
            AppendLine(sb, $"#include \"{settings.ProjectName}.h\"");
        }
        AppendLine(sb, "#include \"StaticLib.h\"");
        AppendLine(sb, "#include \"DynamicLib.h\"");
        AppendLine(sb, string.Empty);
        AppendLine(sb, "int main()");
        AppendLine(sb, "{");
        if (settings.UseIncFolder)
        {
            AppendLine(sb, "    HelloWorld();");
        }
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

        if (settings.UseIncFolder)
        {
            AppendLine(sb, @"set(MY_INC_DIR ""${MY_REPO_DIR}/inc"")");
        }

        if (settings.UseResFolder)
        {
            AppendLine(sb, @"set(MY_RES_DIR ""${MY_REPO_DIR}/res"")");
        }

        if (settings.UseThirdPartyFolder)
        {
            AppendLine(sb, @"set(MY_3RD_DIR ""${MY_REPO_DIR}/3rd"")");
        }

        if (settings.UsePatchFolder)
        {
            AppendLine(sb, @"set(MY_PATCH_DIR ""${MY_REPO_DIR}/patch"")");
        }

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

        AppendLine(sb, "add_subdirectory(src/App)");
        AppendLine(sb, "add_subdirectory(src/Static)");
        AppendLine(sb, "add_subdirectory(src/Dynamic)");

        WriteFile(Path.Combine(settings.ProjectDir, "CMakeLists.txt"), sb.ToString());
    }
}
