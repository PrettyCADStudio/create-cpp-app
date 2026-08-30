namespace create_cpp_app;

public sealed record CMakeProjectSettings
{
    public required string ProjectName { get; init; }
    public required string CppStandard { get; init; }
    public required bool UseIncFolder { get; init; }
    public required bool Force { get; init; }

    public string ProjectDir => Path.Combine(Directory.GetCurrentDirectory(), ProjectName);
    public string SrcDir => Path.Combine(ProjectDir, "src");
    public string AppDir => Path.Combine(SrcDir, "App");
    public string StaticDir => Path.Combine(SrcDir, "Static");
    public string DynamicDir => Path.Combine(SrcDir, "Dynamic");
    public string IncDir => Path.Combine(ProjectDir, "inc");
    public string CmakeDir => Path.Combine(ProjectDir, "cmake");
}
