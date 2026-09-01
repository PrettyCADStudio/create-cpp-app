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

        var useIncFolder = Prompt.Confirm("Create 'inc' folder for shared headers?", defaultValue: false);
        var useResFolder = Prompt.Confirm("Create 'res' folder for resource files?", defaultValue: false);
        var useThirdPartyFolder = Prompt.Confirm("Create '3rd' folder for third-party library files in the project?", defaultValue: false);
        var usePatchFolder = Prompt.Confirm("Create 'patch' folder for patches?", defaultValue: false);
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
            UseIncFolder = useIncFolder,
            UseResFolder = useResFolder,
            UseThirdPartyFolder = useThirdPartyFolder,
            UsePatchFolder = usePatchFolder,
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
        if (settings.UseIncFolder)
        {
            Console.WriteLine($"  Include directory: inc/");
            Console.WriteLine($"  Static library: src/Static/");
            Console.WriteLine($"  Dynamic library: src/Dynamic/");
        }
        if (settings.UseResFolder)
        {
            Console.WriteLine($"  Resource directory: res/");
        }
        if (settings.UseThirdPartyFolder)
        {
            Console.WriteLine($"  Third-party directory: 3rd/");
        }
        if (settings.UsePatchFolder)
        {
            Console.WriteLine($"  Patch directory: patch/");
        }
        if (settings.PythonScripts != PythonScriptMode.None)
        {
            Console.WriteLine($"  Python scripts: {settings.PythonScripts}");
        }
    }
}
