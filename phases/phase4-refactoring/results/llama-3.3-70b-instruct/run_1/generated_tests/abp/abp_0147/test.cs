using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Build;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Minify.Scripts;
using Volo.Abp.Minify.Styles;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests
{
    public class BundlingServiceTests
    {
        private readonly Mock<ILogger<BundlingService>> _loggerMock;
        private readonly Mock<IDotNetProjectBuilder> _dotNetProjectBuilderMock;
        private readonly Mock<IJavascriptMinifier> _jsMinifierMock;
        private readonly Mock<ICssMinifier> _cssMinifierMock;
        private readonly Mock<IScriptBundler> _scriptBundlerMock;
        private readonly Mock<IStyleBundler> _styleBundlerMock;
        private readonly Mock<IConfigReader> _configReaderMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public BundlingServiceTests()
        {
            _loggerMock = new Mock<ILogger<BundlingService>>();
            _dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
            _jsMinifierMock = new Mock<IJavascriptMinifier>();
            _cssMinifierMock = new Mock<ICssMinifier>();
            _scriptBundlerMock = new Mock<IScriptBundler>();
            _styleBundlerMock = new Mock<IStyleBundler>();
            _configReaderMock = new Mock<IConfigReader>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
        }

        [Fact]
        public async Task BundleAsync_LogInformationCalled()
        {
            // Arrange
            var bundlingService = new BundlingService(
                _dotNetProjectBuilderMock.Object,
                _jsMinifierMock.Object,
                _cssMinifierMock.Object,
                _loggerMock.Object,
                _scriptBundlerMock.Object,
                _styleBundlerMock.Object,
                _configReaderMock.Object,
                _cliVersionServiceMock.Object);

            var directory = "test_directory";
            var forceBuild = true;
            var projectType = "WebAssembly";

            // Act
            await bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
