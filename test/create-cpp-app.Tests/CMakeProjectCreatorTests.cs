using System.Text.Json;
using Xunit;

namespace create_cpp_app.Tests;

public class CMakeProjectCreatorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _fixturesDir;
    private readonly string _originalCurrentDir;
    private const string ConfigFileName = "create-cpp-app.json";

    public CMakeProjectCreatorTests()
    {
        _originalCurrentDir = Directory.GetCurrentDirectory();

        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        _fixturesDir = Path.Combine(projectRoot, "test", "fixtures");
        _tempDir = Path.Combine(projectRoot, "temp");

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        Directory.CreateDirectory(_tempDir);

        Directory.SetCurrentDirectory(_tempDir);
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

        var settings = new CMakeProjectSettings
        {
            ProjectName = config.ProjectName,
            CppStandard = config.CppStandard,
            UseIncFolder = config.UseIncFolder,
            Force = false,
        };

        CMakeProjectCreator.Create(settings);

        var generatedDir = Path.Combine(_tempDir, config.ProjectName);
        AssertDirectoriesEqual(fixtureDir, generatedDir);

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
            var expectedContent = File.ReadAllText(Path.Combine(expectedDir, relativePath));
            var actualContent = File.ReadAllText(Path.Combine(actualDir, relativePath));

            Assert.True(expectedContent == actualContent,
                $"Content mismatch: {relativePath}");
        }
    }

    private static List<string> GetRelativeFiles(string dir)
    {
        return Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(dir, f).Replace('\\', '/'))
            .Where(f => f != ConfigFileName)
            .OrderBy(f => f)
            .ToList();
    }

    private sealed class TestConfig
    {
        public string ProjectName { get; set; } = "";
        public string CppStandard { get; set; } = "17";
        public bool UseIncFolder { get; set; }
    }
}
