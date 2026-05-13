using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_LogsInformationOnGeneratingScriptReferences_WhenModeIsNotBundleOrBundleAndMinify()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var styleBundlerMock = new Mock<IStyleBundler>();
            var scriptBundlerMock = new Mock<IScriptBundler>();
            var configReaderMock = new Mock<IConfigReader>();
            var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                Name = null,
                InteractiveAuto = true,
                IsBlazorWebApp = false,
                Parameters = new Dictionary<string, string>()
            };

            var config = new CliConfig
            {
                Bundle = bundleConfig
            };

            configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object,
                StyleBundler = styleBundlerMock.Object,
                ScriptBundler = scriptBundlerMock.Object,
                ConfigReader = configReaderMock.Object,
                DotNetProjectBuilder = dotNetProjectBuilderMock.Object
            };

            // Setup directory and project file
            var testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(testDirectory);
            var csprojPath = Path.Combine(testDirectory, "TestProject.csproj");
            File.WriteAllText(csprojPath, "<Project></Project>");

            // Setup Directory.GetFiles to return the csproj file
            // We cannot mock static methods easily, so we will create a derived class to override behavior
            var bundlingServiceMock = new Mock<BundlingService> { CallBase = true };
            bundlingServiceMock.Object.Logger = loggerMock.Object;
            bundlingServiceMock.Object.StyleBundler = styleBundlerMock.Object;
            bundlingServiceMock.Object.ScriptBundler = scriptBundlerMock.Object;
            bundlingServiceMock.Object.ConfigReader = configReaderMock.Object;
            bundlingServiceMock.Object.DotNetProjectBuilder = dotNetProjectBuilderMock.Object;

            bundlingServiceMock.Setup(s => s.GetTargetFrameworkVersion(It.IsAny<string>(), It.IsAny<string>())).Returns("net6.0");
            bundlingServiceMock.Setup(s => s.GetStartupModule(It.IsAny<string>())).Returns(typeof(object).Assembly);
            bundlingServiceMock.Setup(s => s.FindBundleContributorsRecursively(It.IsAny<Assembly>(), It.IsAny<int>(), It.IsAny<List<BundleTypeDefinition>>()))
                .Callback<Assembly, int, List<BundleTypeDefinition>>((asm, level, list) =>
                {
                    // Add empty list to simulate no contributors
                });
            bundlingServiceMock.Setup(s => s.GenerateStyleDefinitions(It.IsAny<BundleContext>())).Returns("style-definitions");
            bundlingServiceMock.Setup(s => s.GenerateScriptDefinitions(It.IsAny<BundleContext>())).Returns("script-definitions");

            // Act
            await bundlingServiceMock.Object.BundleAsync(testDirectory, forceBuild: false, projectType: BundlingConsts.WebAssembly);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating style references...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating script references...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            File.Delete(csprojPath);
            Directory.Delete(testDirectory);
        }
    }

    // Minimal stub classes to satisfy dependencies
    public class BundleConfig
    {
        public BundlingMode Mode { get; set; }
        public string Name { get; set; }
        public bool InteractiveAuto { get; set; }
        public bool IsBlazorWebApp { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }

    public class CliConfig
    {
        public BundleConfig Bundle { get; set; }
    }

    public enum BundlingMode
    {
        None,
        Bundle,
        BundleAndMinify
    }

    public static class BundlingConsts
    {
        public const string WebAssembly = "WebAssembly";
        public const string MauiBlazor = "MauiBlazor";
    }

    public interface IStyleBundler
    {
        string Bundle(BundleOptions options, BundleContext context);
    }

    public interface IScriptBundler
    {
        string Bundle(BundleOptions options, BundleContext context);
    }

    public interface IConfigReader
    {
        CliConfig Read(string path);
    }

    public interface IDotNetProjectBuilder
    {
        void BuildProjects(List<DotNetProjectInfo> projects, string configuration);
    }

    public class BundleOptions
    {
        public string Directory { get; set; }
        public string FrameworkVersion { get; set; }
        public string ProjectFileName { get; set; }
        public string BundleName { get; set; }
        public bool Minify { get; set; }
        public string ProjectType { get; set; }
    }

    public class BundleContext
    {
        public Dictionary<string, string> Parameters { get; set; }
        public bool InteractiveAuto { get; set; }
    }

    public class DotNetProjectInfo
    {
        public DotNetProjectInfo(string name, string filePath, bool isMain)
        {
            Name = name;
            FilePath = filePath;
            IsMain = isMain;
        }

        public string Name { get; }
        public string FilePath { get; }
        public bool IsMain { get; }
    }
}
