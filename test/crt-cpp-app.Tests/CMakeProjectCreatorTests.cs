using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace crt_cpp_app.Tests;

public class CMakeProjectCreatorTests : IDisposable
{
    private static readonly string _projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private readonly string _runDir = Path.Combine(Path.GetTempPath(), "crt-cpp-app-tests", Guid.NewGuid().ToString("N"));

    private readonly string _fixturesDir;
    private const string ConfigFileName = "crt-cpp-app.json";

    public CMakeProjectCreatorTests()
    {
        _fixturesDir = Path.Combine(_projectRoot, "test", "fixtures");
        Directory.CreateDirectory(_runDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_runDir))
        {
            Directory.Delete(_runDir, recursive: true);
        }
    }

    public static IEnumerable<object[]> GetFixtureCases()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var fixturesDir = Path.Combine(projectRoot, "test", "fixtures");

        foreach (var dir in Directory.GetDirectories(fixturesDir))
        {
            var configFile = Path.Combine(dir, ConfigFileName);
            if (File.Exists(configFile))
            {
                var name = Path.GetFileName(dir);
                yield return new object[] { name };
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetFixtureCases))]
    public void Create_MatchesFixture(string fixtureName)
    {
        var fixtureDir = Path.Combine(_fixturesDir, fixtureName);
        var configFile = Path.Combine(fixtureDir, ConfigFileName);

        var config = JsonSerializer.Deserialize<TestConfig>(
            File.ReadAllText(configFile),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Console.WriteLine($"[Test] Fixture: {fixtureName}");
        Console.WriteLine($"  ProjectName:  {config.ProjectName}");
        Console.WriteLine($"  CppStandard:  {config.CppStandard}");

        var settings = new CMakeProjectSettings
        {
            ProjectName = config.ProjectName,
            CppStandard = config.CppStandard,
            Force = false,
            InitializeGit = false,
            OutputDirectory = _runDir,
        };

        CMakeProjectCreator.Create(settings);

        var generatedDir = Path.Combine(_runDir, config.ProjectName);
        Assert.False(Directory.Exists(Path.Combine(generatedDir, ".git")));
        AssertDirectoriesEqual(fixtureDir, generatedDir);
        AssertCMakeProjectBuilds(generatedDir);

        Console.WriteLine($"[Test] Result:  PASSED");
    }

    [Theory]
    [InlineData("..")]
    [InlineData("nested/project")]
    [InlineData("name with spaces")]
    [InlineData("name;set(EVIL ON)")]
    public void Create_RejectsUnsafeProjectName(string projectName)
    {
        var settings = new CMakeProjectSettings
        {
            ProjectName = projectName,
            CppStandard = "17",
            Force = false,
            InitializeGit = false,
            OutputDirectory = _runDir,
        };

        Assert.Throws<ArgumentException>(() => CMakeProjectCreator.Create(settings));
    }

    [Theory]
    [InlineData(PythonScriptMode.Direct)]
    [InlineData(PythonScriptMode.Pipenv)]
    public void Create_GeneratesRequestedPythonDevelopmentScripts(PythonScriptMode scriptMode)
    {
        var projectName = $"PythonScripts{scriptMode}";
        var settings = new CMakeProjectSettings
        {
            ProjectName = projectName,
            CppStandard = "17",
            Force = false,
            PythonScripts = scriptMode,
            InitializeGit = false,
            OutputDirectory = _runDir,
        };

        CMakeProjectCreator.Create(settings);

        var projectDir = Path.Combine(_runDir, projectName);
        var scriptDir = scriptMode == PythonScriptMode.Pipenv
            ? Path.Combine(projectDir, "scripts")
            : projectDir;
        foreach (var scriptName in new[] { "mksln.py", "build.py", "install.py", "build-install.py", "archive.py" })
        {
            Assert.True(File.Exists(Path.Combine(scriptDir, scriptName)), $"Missing {scriptName}");
        }

        Assert.Equal(scriptMode == PythonScriptMode.Pipenv, File.Exists(Path.Combine(projectDir, "Pipfile")));
        if (scriptMode == PythonScriptMode.Pipenv)
        {
            Assert.Contains("build-install = \"python scripts/build-install.py\"", File.ReadAllText(Path.Combine(projectDir, "Pipfile")));
        }
    }

    private static void AssertDirectoriesEqual(string expectedDir, string actualDir)
    {
        var expectedFiles = GetRelativeFiles(expectedDir);
        var actualFiles = GetRelativeFiles(actualDir);

        Assert.True(expectedFiles.SequenceEqual(actualFiles),
            $"File list mismatch.\n  Expected: [{string.Join(", ", expectedFiles)}]\n  Actual:   [{string.Join(", ", actualFiles)}]");

        foreach (var relativePath in expectedFiles)
        {
            var expectedLines = File.ReadAllLines(Path.Combine(expectedDir, relativePath));
            var actualLines = File.ReadAllLines(Path.Combine(actualDir, relativePath));

            Assert.True(expectedLines.SequenceEqual(actualLines),
                $"Content mismatch: {relativePath}\nExpected:\n{string.Join("\n", expectedLines)}\nActual:\n{string.Join("\n", actualLines)}");
        }
    }

    private static void AssertCMakeProjectBuilds(string projectDir)
    {
        var cmakeCommand = "cmake";
        var buildDir = Path.Combine(projectDir, "build");

        var configureResult = RunProcess(cmakeCommand, $"-S \"{projectDir}\" -B \"{buildDir}\"");
        Assert.True(
            configureResult.ExitCode == 0,
            $"CMake configure failed for {projectDir}.\nSTDOUT:\n{configureResult.StdOut}\nSTDERR:\n{configureResult.StdErr}");

        var buildResult = RunProcess(cmakeCommand, $"--build \"{buildDir}\" --config Release");
        Assert.True(
            buildResult.ExitCode == 0,
            $"CMake build failed for {projectDir}.\nSTDOUT:\n{buildResult.StdOut}\nSTDERR:\n{buildResult.StdErr}");
    }

    private static ProcessResult RunProcess(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start process '{fileName}'.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    private static List<string> GetRelativeFiles(string dir)
    {
        return Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(dir, f).Replace('\\', '/'))
            .Where(f => f != ConfigFileName && !f.StartsWith(".git/", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToList();
    }

    private sealed record ProcessResult(int ExitCode, string StdOut, string StdErr);

    private sealed class TestConfig
    {
        public string ProjectName { get; set; } = "";
        public string CppStandard { get; set; } = "17";
    }
}
