using System.CommandLine;

namespace crt_cpp_app;

public static class Program
{
    public static int Main(string[] args)
    {
        var forceOption = new Option<bool>("--force", "-f")
        {
            Description = "Force delete existing project directory and recreate",
        };
        var whereOption = new Option<bool>("--where")
        {
            Description = "Print the path of the running crt-cpp-app executable",
        };

        var rootCommand = new RootCommand("Create a new C++ CMake project")
        {
            forceOption,
            whereOption,
        };

        rootCommand.SetAction((result) =>
        {
            if (result.GetValue(whereOption))
            {
                Console.WriteLine(Environment.ProcessPath);
                return;
            }

            var force = result.GetValue(forceOption);
            Run(force);
        });

        return rootCommand.Parse(args).Invoke();
    }

    private static void Run(bool force)
    {
        var settings = UserInteraction.PromptSettings() with { Force = force };
        settings.Validate();

        if (Directory.Exists(settings.ProjectDir))
        {
            if (settings.Force || UserInteraction.ConfirmOverwrite(settings.ProjectName))
            {
                Directory.Delete(settings.ProjectDir, recursive: true);
            }
            else
            {
                Console.WriteLine("Aborted.");
                return;
            }
        }

        CMakeProjectCreator.Create(settings);
        UserInteraction.PrintSummary(settings);
    }
}
