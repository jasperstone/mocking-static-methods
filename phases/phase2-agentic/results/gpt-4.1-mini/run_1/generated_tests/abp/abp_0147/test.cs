using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_LogsInformation_WhenGeneratingScriptReferences()
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

            var config = new CliConfiguration
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

            // Setup StyleBundler and ScriptBundler to return dummy strings
            styleBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("style bundle");
            scriptBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("script bundle");

            // Setup GenerateStyleDefinitions and GenerateScriptDefinitions to return dummy strings
            // These are private methods, so we cannot mock them directly.
            // Instead, we will test the logging calls that happen in the else branch (Mode == None).

            // We need to create a temporary directory with a dummy .csproj file to avoid exceptions
            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempDir);
            var csprojPath = System.IO.Path.Combine(tempDir, "TestProject.csproj");
            System.IO.File.WriteAllText(csprojPath, "<Project></Project>");

            // We will override the Directory.GetFiles method by using a wrapper or by setting the projectFilePath manually.
            // Since we cannot override static methods easily, we will simulate the environment by creating the file.

            // Act
            // We call BundleAsync with forceBuild = false and projectType = WebAssembly (default)
            // This will trigger the else branch and call Logger.LogInformation("Generating script references...");
            await bundlingService.BundleAsync(tempDir, false);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating script references...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            System.IO.File.Delete(csprojPath);
            System.IO.Directory.Delete(tempDir);
        }
    }
}
