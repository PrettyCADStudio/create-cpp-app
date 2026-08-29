using Sharprompt;

namespace create_cpp_app;

public static class UserInteraction
{
    public static CMakeProjectSettings PromptSettings()
    {
        var projectName = Prompt.Input<string>("Project name",
            validators: new[] { Validators.Required() });

        var cppStandard = Prompt.Select("C++ standard", new[] { "17", "20" }, defaultValue: "17");

        var useIncFolder = Prompt.Confirm("Add 'inc' folder for shared headers?", defaultValue: false);

        return new CMakeProjectSettings
        {
            ProjectName = projectName,
            CppStandard = cppStandard,
            UseIncFolder = useIncFolder,
            Force = false,
        };
    }

    public static bool ConfirmOverwrite(string projectName)
    {
        return Prompt.Confirm($"Directory '{projectName}' already exists. Delete and recreate?");
    }

    public static void PrintSummary(CMakeProjectSettings settings)
    {
        Console.WriteLine();
        Console.WriteLine($"Project '{settings.ProjectName}' created at {settings.ProjectDir}");
        Console.WriteLine($"  C++ standard: C++{settings.CppStandard}");
        Console.WriteLine($"  Output directory: bin/");
        if (settings.UseIncFolder)
            Console.WriteLine($"  Include directory: inc/");
    }
}
