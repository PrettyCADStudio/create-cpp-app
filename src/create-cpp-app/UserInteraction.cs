using Sharprompt;

namespace create_cpp_app;

public static class UserInteraction
{
    private const string ProjectNamePattern = "^[A-Za-z_][A-Za-z0-9_-]*$";
    private const string ProjectNameRequirement = "Project name must start with a letter or underscore and may contain only letters, digits, underscores, and hyphens.";

    public static CMakeProjectSettings PromptSettings()
    {
        var projectName = PromptProjectName();

        var cppStandard = Prompt.Select("C++ standard", new[] { "17", "20" }, defaultValue: "17");

        var pythonScripts = Prompt.Select(
            "Python development scripts",
            new[] { "Do not use Python scripts", "Use Python scripts directly", "Use Pipenv" },
            defaultValue: "Do not use Python scripts") switch
        {
            "Use Python scripts directly" => PythonScriptMode.Direct,
            "Use Pipenv" => PythonScriptMode.Pipenv,
            _ => PythonScriptMode.None,
        };

        return new CMakeProjectSettings
        {
            ProjectName = projectName,
            CppStandard = cppStandard,
            PythonScripts = pythonScripts,
            Force = false,
        };
    }

    private static string PromptProjectName()
    {
        while (true)
        {
            var projectName = Prompt.Input<string>("Project name", validators: new[] { Validators.Required() });
            if (System.Text.RegularExpressions.Regex.IsMatch(projectName, ProjectNamePattern))
            {
                return projectName;
            }

            Console.WriteLine(ProjectNameRequirement);
        }
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
        Console.WriteLine($"  Include directory: inc/");
        Console.WriteLine($"  Resource directory: res/");
        Console.WriteLine($"  Third-party directory: 3rd/");
        Console.WriteLine($"  Patch directory: patch/");
        Console.WriteLine($"  Static library: src/Static/");
        Console.WriteLine($"  Dynamic library: src/Dynamic/");
        if (settings.PythonScripts != PythonScriptMode.None)
        {
            Console.WriteLine($"  Python scripts: {settings.PythonScripts}");
        }
    }
}
