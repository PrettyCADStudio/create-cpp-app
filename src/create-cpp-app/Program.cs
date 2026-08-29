namespace create_cpp_app;

public static class Program
{
    public static void Main(string[] args)
    {
        var settings = UserInteraction.PromptSettings();
        CMakeProjectCreator.Create(settings);
        UserInteraction.PrintSummary(settings);
    }
}
