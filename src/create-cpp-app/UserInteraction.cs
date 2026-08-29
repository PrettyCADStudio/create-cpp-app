using Sharprompt;

namespace create_cpp_app;

public static class UserInteraction
{
    public static CMakeProjectSettings PromptSettings()
    {
        var projectName = Prompt.Input<string>("Project name",
            validators: new[] { Validators.Required() });

        var cppStandard = Prompt.Select("C++ standard", new[] { "17", "20" }, defaultValue: "17");

        return new CMakeProjectSettings
        {
            ProjectName = projectName,
            CppStandard = cppStandard,
        };
    }

    public static void PrintSummary(CMakeProjectSettings settings)
    {
        Console.WriteLine();
        Console.WriteLine($"Project '{settings.ProjectName}' created at {settings.ProjectDir}");
        Console.WriteLine($"  C++ standard: C++{settings.CppStandard}");
        Console.WriteLine($"  Output directory: bin/");
    }
}
