using Sharprompt;

var projectName = Prompt.Input<string>("Project name",
    validators: new[] { Validators.Required() });

var cppStandard = Prompt.Select("C++ standard", new[] { "17", "20" }, defaultValue: "17");

var projectDir = Path.Combine(Directory.GetCurrentDirectory(), projectName);
var srcDir = Path.Combine(projectDir, "src");
var appDir = Path.Combine(srcDir, "App");

Directory.CreateDirectory(appDir);

// Solution CMakeLists.txt
File.WriteAllText(Path.Combine(projectDir, "CMakeLists.txt"),
$@"cmake_minimum_required(VERSION 3.20)
project({projectName} LANGUAGES CXX)

set(CMAKE_CXX_STANDARD {cppStandard})
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${{CMAKE_SOURCE_DIR}}/bin)

add_subdirectory(src/App)
");

// App CMakeLists.txt
File.WriteAllText(Path.Combine(appDir, "CMakeLists.txt"),
$@"add_executable(App main.cpp)
");

// main.cpp
File.WriteAllText(Path.Combine(appDir, "main.cpp"),
@"#include <iostream>

int main() {
    std::cout << ""Hello, World!"" << std::endl;
    return 0;
}
");

Console.WriteLine();
Console.WriteLine($"Project '{projectName}' created at {projectDir}");
Console.WriteLine($"  C++ standard: C++{cppStandard}");
Console.WriteLine($"  Output directory: bin/");
