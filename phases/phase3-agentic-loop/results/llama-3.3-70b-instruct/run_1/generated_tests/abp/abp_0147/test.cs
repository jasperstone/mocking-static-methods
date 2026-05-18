using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Core;
using Xunit;

namespace Volo.Abp.Cli.Tests
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

            var directory = "test-directory";
            var forceBuild = true;
            var projectType = "WebAssembly";

            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(new BundleConfig());

            // Act
            await bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.Log(It.Is<LogLevel>(ll => ll == LogLevel.Information), It.IsAny<EventId>(), It.IsAny<ITimestamp>(), It.IsAny<Exception>(), It.IsAny<Func<ITimestamp, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
