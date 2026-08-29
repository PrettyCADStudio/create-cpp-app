namespace create_cpp_app;

public static class CMakeProjectCreator
{
    public static void Create(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.AppDir);

        if (settings.UseIncFolder)
            Directory.CreateDirectory(settings.IncDir);

        WriteRootCMakeLists(settings);
        WriteAppCMakeLists(settings);
        WriteMainCpp(settings);

        if (settings.UseIncFolder)
            WriteHelloWorldHeader(settings);
    }

    private static void WriteRootCMakeLists(CMakeProjectSettings settings)
    {
        File.WriteAllText(Path.Combine(settings.ProjectDir, "CMakeLists.txt"),
$@"cmake_minimum_required(VERSION 3.20)
project({settings.ProjectName} LANGUAGES CXX)

set(CMAKE_CXX_STANDARD {settings.CppStandard})
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${{CMAKE_SOURCE_DIR}}/bin)
{(settings.UseIncFolder ? "\ninclude_directories(inc)\n" : "")}
add_subdirectory(src/App)
");
    }

    private static void WriteAppCMakeLists(CMakeProjectSettings settings)
    {
        File.WriteAllText(Path.Combine(settings.AppDir, "CMakeLists.txt"),
@"add_executable(App main.cpp)
");
    }

    private static void WriteHelloWorldHeader(CMakeProjectSettings settings)
    {
        File.WriteAllText(Path.Combine(settings.IncDir, "HelloWorld.h"),
$@"#ifndef HELLO_WORLD_H
#define HELLO_WORLD_H

#include <string>

inline std::string HelloWorld()
{{
    return ""Hello, World!"";
}}

#endif // HELLO_WORLD_H
");
    }

    private static void WriteMainCpp(CMakeProjectSettings settings)
    {
        File.WriteAllText(Path.Combine(settings.AppDir, "main.cpp"),
@"#include <iostream>

int main() {
    std::cout << ""Hello, World!"" << std::endl;
    return 0;
}
");
    }
}
