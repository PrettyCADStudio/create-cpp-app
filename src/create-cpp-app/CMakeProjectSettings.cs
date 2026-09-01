namespace create_cpp_app;

public enum PythonScriptMode
{
    None,
    Direct,
    Pipenv,
}

public sealed record CMakeProjectSettings
{
    public required string ProjectName { get; init; }
    public required string CppStandard { get; init; }
    public required bool UseIncFolder { get; init; }
    public required bool UseResFolder { get; init; }
    public required bool UseThirdPartyFolder { get; init; }
    public required bool UsePatchFolder { get; init; }
    public required bool Force { get; init; }
    public PythonScriptMode PythonScripts { get; init; } = PythonScriptMode.None;
    public bool InitializeGit { get; init; } = true;

    // This is primarily useful to callers embedding the generator (including tests).
    // The CLI intentionally leaves it at the process working directory.
    public string OutputDirectory { get; init; } = Directory.GetCurrentDirectory();

    public string ProjectDir => Path.Combine(OutputDirectory, ProjectName);
    public string SrcDir => Path.Combine(ProjectDir, "src");
    public string AppDir => Path.Combine(SrcDir, "App");
    public string StaticDir => Path.Combine(SrcDir, "Static");
    public string DynamicDir => Path.Combine(SrcDir, "Dynamic");
    public string IncDir => Path.Combine(ProjectDir, "inc");
    public string ResDir => Path.Combine(ProjectDir, "res");
    public string ThirdPartyDir => Path.Combine(ProjectDir, "3rd");
    public string PatchDir => Path.Combine(ProjectDir, "patch");
    public string CmakeDir => Path.Combine(ProjectDir, "cmake");

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ProjectName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(ProjectName, "^[A-Za-z_][A-Za-z0-9_-]*$"))
        {
            throw new ArgumentException(
                "Project name must start with a letter or underscore and contain only letters, digits, underscores, or hyphens.",
                nameof(ProjectName));
        }

        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            throw new ArgumentException("Output directory must not be empty.", nameof(OutputDirectory));
        }
    }
}
