using System.Text;

namespace create_cpp_app;

public static class CMakeProjectCreator
{
    public static void Create(CMakeProjectSettings settings)
    {
        CreateIncFolder(settings);
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
            sb.AppendLine("#pragma once");
            sb.AppendLine();
            sb.AppendLine("#include <iostream>");
            sb.AppendLine();
            sb.AppendLine("inline void HelloWorld()");
            sb.AppendLine("{");
            sb.AppendLine("    std::cout << \"Hello World\" << std::endl;");
            sb.AppendLine("}");
            File.WriteAllText(Path.Combine(settings.IncDir, $"{settings.ProjectName}.h"), sb.ToString());
        }
    }

    private static void CreateCMakeFolder(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.CmakeDir);

        var sb = new StringBuilder();
        sb.AppendLine($"# {settings.ProjectName} cmake functions");
        File.WriteAllText(Path.Combine(settings.CmakeDir, $"{settings.ProjectName}.cmake"), sb.ToString());

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
        sb.AppendLine("#pragma once");
        sb.AppendLine();
        sb.AppendLine("void HelloStatic();");

        var path = Path.Combine(settings.StaticDir, "Public", "StaticLib.h");
        File.WriteAllText(path, sb.ToString());
    }

    private static void CreateStaticSourceFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#include \"StaticLib.h\"");
        sb.AppendLine("#include <iostream>");
        sb.AppendLine();
        sb.AppendLine("void HelloStatic()");
        sb.AppendLine("{");
        sb.AppendLine("    std::cout << \"Hello Static Library\" << std::endl;");
        sb.AppendLine("}");

        var path = Path.Combine(settings.StaticDir, "Private", "StaticLib.cpp");
        File.WriteAllText(path, sb.ToString());
    }

    private static void CreateStaticCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("add_library(Static STATIC Private/StaticLib.cpp)");
        sb.AppendLine("target_include_directories(Static PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/Public)");

        var path = Path.Combine(settings.StaticDir, "CMakeLists.txt");
        File.WriteAllText(path, sb.ToString());
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
        sb.AppendLine("#pragma once");
        sb.AppendLine();
        sb.AppendLine("/* Cross-platform export macro */");
        sb.AppendLine("#if defined(_WIN32) || defined(__CYGWIN__)");
        sb.AppendLine("  #ifdef Dynamic_EXPORTS");
        sb.AppendLine("    #define DYNAMIC_API __declspec(dllexport)");
        sb.AppendLine("  #else");sb.AppendLine("    #define DYNAMIC_API __declspec(dllimport)");
        sb.AppendLine("  #endif");
        sb.AppendLine("#elif defined(__GNUC__) && __GNUC__ >= 4");
        sb.AppendLine("  #define DYNAMIC_API __attribute__ ((visibility (\"default\")))");
        sb.AppendLine("#else");
        sb.AppendLine("  #define DYNAMIC_API");
        sb.AppendLine("#endif");

        var path = Path.Combine(settings.DynamicDir, "Public", "DynamicExports.h");
        File.WriteAllText(path, sb.ToString());
    }

    private static void CreateDynamicHeaderFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#pragma once");
        sb.AppendLine();
        sb.AppendLine("#include \"DynamicExports.h\"");
        sb.AppendLine();
        sb.AppendLine("DYNAMIC_API void HelloDynamic();");

        var path = Path.Combine(settings.DynamicDir, "Public", "DynamicLib.h");
        File.WriteAllText(path, sb.ToString());
    }

        private static void CreateDynamicSourceFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#include \"DynamicLib.h\"");
        sb.AppendLine();
        sb.AppendLine("#include <iostream>");
        sb.AppendLine();
        sb.AppendLine("void HelloDynamic()");
        sb.AppendLine("{");
        sb.AppendLine("    std::cout << \"Hello Dynamic Library\" << std::endl;");
        sb.AppendLine("}");

        var path = Path.Combine(settings.DynamicDir, "Private", "DynamicLib.cpp");
        File.WriteAllText(path, sb.ToString());
    }

    private static void CreateDynamicCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("add_library(Dynamic SHARED Private/DynamicLib.cpp)");
        sb.AppendLine("target_include_directories(Dynamic PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/Public)");

        var path = Path.Combine(settings.DynamicDir, "CMakeLists.txt");
        File.WriteAllText(path, sb.ToString());
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
        sb.AppendLine("add_executable(App main.cpp)");
        if (settings.UseIncFolder)
        {
            sb.AppendLine("include_directories(${MY_INC_DIR})");
        }
        sb.AppendLine("target_link_libraries(App PRIVATE Static)");
        sb.AppendLine("target_link_libraries(App PRIVATE Dynamic)");

        var path = Path.Combine(settings.AppDir, "CMakeLists.txt");
        File.WriteAllText(path, sb.ToString());
    }

    private static void CreateAppMainFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#include <iostream>");
        if (settings.UseIncFolder)
        {
            sb.AppendLine($"#include \"{settings.ProjectName}.h\"");
        }
        sb.AppendLine("#include \"StaticLib.h\"");
        sb.AppendLine("#include \"DynamicLib.h\"");
        sb.AppendLine();
        sb.AppendLine("int main()");
        sb.AppendLine("{");
        if (settings.UseIncFolder)
        {
            sb.AppendLine("    HelloWorld();");
        }
        sb.AppendLine("    HelloStatic();");
        sb.AppendLine("    HelloDynamic();");
        sb.AppendLine("    return 0;");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(settings.AppDir, "main.cpp"), sb.ToString());
    }

    private static void CreateSolutionCMakeFile(CMakeProjectSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("cmake_minimum_required(VERSION 3.20)");
        sb.AppendLine($"project({settings.ProjectName} LANGUAGES CXX)");
        sb.AppendLine();
        sb.AppendLine($"set(CMAKE_CXX_STANDARD {settings.CppStandard})");
        sb.AppendLine("set(CMAKE_CXX_STANDARD_REQUIRED ON)");
        sb.AppendLine();
        sb.AppendLine(@"set(MY_SOURCE_DIR ""${CMAKE_CURRENT_SOURCE_DIR}"")");
        sb.AppendLine(@"set(MY_BINARY_DIR ""${MY_SOURCE_DIR}/bin"")");
        sb.AppendLine(@"set(MY_BUILD_DIR ""${MY_SOURCE_DIR}/build"")");
        sb.AppendLine(@"set(MY_CMAKE_DIR ""${MY_SOURCE_DIR}/cmake"")");

        if (settings.UseIncFolder)
        {
            sb.AppendLine(@"set(MY_INC_DIR ""${MY_SOURCE_DIR}/inc"")");
        }

        sb.AppendLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        sb.AppendLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        sb.AppendLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ""${MY_BINARY_DIR}"")");
        sb.AppendLine();
        sb.AppendLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        sb.AppendLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        sb.AppendLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        sb.AppendLine(@"set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        sb.AppendLine();
        sb.AppendLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        sb.AppendLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        sb.AppendLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        sb.AppendLine(@"set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        sb.AppendLine();
        sb.AppendLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_DEBUG ""${MY_BINARY_DIR}/Debug"")");
        sb.AppendLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_RELEASE ""${MY_BINARY_DIR}/Release"")");
        sb.AppendLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_MINSIZEREL ""${MY_BINARY_DIR}/MinSizeRel"")");
        sb.AppendLine(@"set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${MY_BINARY_DIR}/RelWithDebInfo"")");
        sb.AppendLine();
        sb.AppendLine($"include(${{MY_CMAKE_DIR}}/{settings.ProjectName}.cmake)");
        sb.AppendLine();

        sb.AppendLine("add_subdirectory(src/App)");
        sb.AppendLine("add_subdirectory(src/Static)");
        sb.AppendLine("add_subdirectory(src/Dynamic)");

        File.WriteAllText(Path.Combine(settings.ProjectDir, "CMakeLists.txt"), sb.ToString());
    }
}





