namespace create_cpp_app;

public sealed record CMakeProjectSettings
{
    public required string ProjectName { get; init; }
    public required string CppStandard { get; init; }
    public required bool UseIncFolder { get; init; }
    public required bool UseResFolder { get; init; }
    public required bool UseThirdPartyFolder { get; init; }
    public required bool UsePatchFolder { get; init; }
    public required bool Force { get; init; }

    public string ProjectDir => Path.Combine(Directory.GetCurrentDirectory(), ProjectName);
    public string SrcDir => Path.Combine(ProjectDir, "src");
    public string AppDir => Path.Combine(SrcDir, "App");
    public string StaticDir => Path.Combine(SrcDir, "Static");
    public string DynamicDir => Path.Combine(SrcDir, "Dynamic");
    public string IncDir => Path.Combine(ProjectDir, "inc");
    public string ResDir => Path.Combine(ProjectDir, "res");
    public string ThirdPartyDir => Path.Combine(ProjectDir, "3rd");
    public string PatchDir => Path.Combine(ProjectDir, "patch");
    public string CmakeDir => Path.Combine(ProjectDir, "cmake");
}
