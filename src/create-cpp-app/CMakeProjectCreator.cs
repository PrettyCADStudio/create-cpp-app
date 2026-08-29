namespace create_cpp_app;

public static class CMakeProjectCreator
{
    public static void Create(CMakeProjectSettings settings)
    {
        Directory.CreateDirectory(settings.AppDir);

        if (settings.UseIncFolder)
            Directory.CreateDirectory(settings.IncDir);

        Directory.CreateDirectory(settings.CmakeDir);

        WriteRootCMakeLists(settings);
        WriteAppCMakeLists(settings);
        WriteMainCpp(settings);

        WriteProjectCmake(settings);

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

set(MY_SOURCE_DIR ""${{CMAKE_CURRENT_SOURCE_DIR}}"")
set(MY_BINARY_DIR ""${{MY_SOURCE_DIR}}/bin"")
set(MY_BUILD_DIR ""${{MY_SOURCE_DIR}}/build"")
set(MY_CMAKE_DIR ""${{MY_SOURCE_DIR}}/cmake"")
{(settings.UseIncFolder ? "set(MY_INC_DIR \"${MY_SOURCE_DIR}/inc\")\ninclude_directories(${MY_INC_DIR})\n\n" : "")}set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ""${{MY_BINARY_DIR}}"")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ""${{MY_BINARY_DIR}}"")
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ""${{MY_BINARY_DIR}}"")

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG ""${{MY_BINARY_DIR}}/Debug"")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE ""${{MY_BINARY_DIR}}/Release"")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL ""${{MY_BINARY_DIR}}/MinSizeRel"")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${{MY_BINARY_DIR}}/RelWithDebInfo"")

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_DEBUG ""${{MY_BINARY_DIR}}/Debug"")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELEASE ""${{MY_BINARY_DIR}}/Release"")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_MINSIZEREL ""${{MY_BINARY_DIR}}/MinSizeRel"")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${{MY_BINARY_DIR}}/RelWithDebInfo"")

set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_DEBUG ""${{MY_BINARY_DIR}}/Debug"")
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_RELEASE ""${{MY_BINARY_DIR}}/Release"")
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_MINSIZEREL ""${{MY_BINARY_DIR}}/MinSizeRel"")
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_RELWITHDEBINFO ""${{MY_BINARY_DIR}}/RelWithDebInfo"")

include(${{MY_CMAKE_DIR}}/{settings.ProjectName}.cmake)

add_subdirectory(src/App)
");
    }

    private static void WriteProjectCmake(CMakeProjectSettings settings)
    {
        File.WriteAllText(Path.Combine(settings.CmakeDir, $"{settings.ProjectName}.cmake"),
$@"# {settings.ProjectName} cmake functions
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
