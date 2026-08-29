namespace create_cpp_app;

public sealed class CMakeProjectSettings
{
    public required string ProjectName { get; init; }
    public required string CppStandard { get; init; }

    public string ProjectDir => Path.Combine(Directory.GetCurrentDirectory(), ProjectName);
    public string SrcDir => Path.Combine(ProjectDir, "src");
    public string AppDir => Path.Combine(SrcDir, "App");
}
