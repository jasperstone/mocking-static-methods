using Xunit;
using Moq;
using Volo.Abp.Cli.Bundling;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Build;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Volo.Abp.Cli.Bundling.Tests
{
    public class BundlingServiceTests
    {
        private readonly Mock<ILogger<BundlingService>> _loggerMock;
        private readonly Mock<IStyleBundler> _styleBundlerMock;
        private readonly Mock<IScriptBundler> _scriptBundlerMock;
        private readonly Mock<IConfigReader> _configReaderMock;
        private readonly Mock<IDotNetProjectBuilder> _dotNetProjectBuilderMock;
        private readonly BundlingService _bundlingService;

        public BundlingServiceTests()
        {
            _loggerMock = new Mock<ILogger<BundlingService>>();
            _styleBundlerMock = new Mock<IStyleBundler>();
            _scriptBundlerMock = new Mock<IScriptBundler>();
            _configReaderMock = new Mock<IConfigReader>();
            _dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();

            _bundlingService = new BundlingService
            {
                Logger = _loggerMock.Object,
                StyleBundler = _styleBundlerMock.Object,
                ScriptBundler = _scriptBundlerMock.Object,
                ConfigReader = _configReaderMock.Object,
                DotNetProjectBuilder = _dotNetProjectBuilderMock.Object
            };
        }

        [Fact]
        public async Task BundleAsync_ShouldLogInformation_WhenGeneratingScriptReferences()
        {
            // Arrange
            var directory = "testDirectory";
            var projectType = "WebAssembly";
            var bundleConfig = new BundleConfig { Mode = BundlingMode.Bundle };
            var config = new CliConfig { Bundle = bundleConfig };
            var projectFilePath = "testProject.csproj";
            var projectFiles = new[] { projectFilePath };
            var bundleDefinitions = new List<BundleTypeDefinition>();
            var styleContext = new BundleContext();
            var scriptContext = new BundleContext();

            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);
            _styleBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("styleDefinitions");
            _scriptBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("scriptDefinitions");

            Directory.SetCurrentDirectory(directory);
            Directory.CreateDirectory(directory);
            File.WriteAllText(projectFilePath, "<Project></Project>");

            // Act
            await _bundlingService.BundleAsync(directory, false, projectType);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Generating script references..."),
                Times.Once);
        }
    }
}
