using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace create_cpp_app.Tests;

public class CMakeProjectCreatorTests : IDisposable
{
    private static readonly string _projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private static readonly string _tempRoot = Path.Combine(_projectRoot, "temp");
    private static readonly string _runDir = Path.Combine(_tempRoot, $"test-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}");

    private readonly string _fixturesDir;
    private readonly string _originalCurrentDir;
    private const string ConfigFileName = "create-cpp-app.json";

    public CMakeProjectCreatorTests()
    {
        _originalCurrentDir = Directory.GetCurrentDirectory();
        _fixturesDir = Path.Combine(_projectRoot, "test", "fixtures");

        Directory.CreateDirectory(_tempRoot);

        foreach (var legacyDir in Directory.EnumerateDirectories(_tempRoot))
        {
            var dirName = Path.GetFileName(legacyDir);
            if (!dirName.StartsWith("test-", StringComparison.OrdinalIgnoreCase))
            {
                Directory.Delete(legacyDir, recursive: true);
            }
        }

        foreach (var legacyDir in Directory.EnumerateDirectories(_tempRoot))
        {
            var dirName = Path.GetFileName(legacyDir);
            if (dirName.StartsWith("test-", StringComparison.OrdinalIgnoreCase) && !string.Equals(dirName, Path.GetFileName(_runDir), StringComparison.OrdinalIgnoreCase))
            {
                Directory.Delete(legacyDir, recursive: true);
            }
        }

        Directory.CreateDirectory(_runDir);
        Directory.SetCurrentDirectory(_runDir);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalCurrentDir);
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
        Console.WriteLine($"  UseIncFolder: {config.UseIncFolder}");
        Console.WriteLine($"  UseResFolder: {config.UseResFolder}");
        Console.WriteLine($"  UseThirdPartyFolder: {config.UseThirdPartyFolder}");
        Console.WriteLine($"  UsePatchFolder: {config.UsePatchFolder}");

        var settings = new CMakeProjectSettings
        {
            ProjectName = config.ProjectName,
            CppStandard = config.CppStandard,
            UseIncFolder = config.UseIncFolder,
            UseResFolder = config.UseResFolder,
            UseThirdPartyFolder = config.UseThirdPartyFolder,
            UsePatchFolder = config.UsePatchFolder,
            Force = false,
        };

        CMakeProjectCreator.Create(settings);

        var generatedDir = Path.Combine(_runDir, config.ProjectName);
        AssertDirectoriesEqual(fixtureDir, generatedDir);
        AssertCMakeProjectBuilds(generatedDir);

        Console.WriteLine($"[Test] Result:  PASSED");
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
                $"Content mismatch: {relativePath}");
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
            .Where(f => f != ConfigFileName)
            .OrderBy(f => f)
            .ToList();
    }

    private sealed record ProcessResult(int ExitCode, string StdOut, string StdErr);

    private sealed class TestConfig
    {
        public string ProjectName { get; set; } = "";
        public string CppStandard { get; set; } = "17";
        public bool UseIncFolder { get; set; }
        public bool UseResFolder { get; set; }
        public bool UseThirdPartyFolder { get; set; }
        public bool UsePatchFolder { get; set; }
    }
}
