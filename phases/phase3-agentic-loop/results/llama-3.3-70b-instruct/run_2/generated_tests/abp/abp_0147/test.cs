using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class BundlingServiceTests
    {
        private readonly Mock<ILogger<BundlingService>> _loggerMock;

        public BundlingServiceTests()
        {
            _loggerMock = new Mock<ILogger<BundlingService>>();
        }

        [Fact]
        public async Task BundleAsync_LogInformation_Called()
        {
            // Arrange
            var bundlingService = new BundlingService(
                _loggerMock.Object,
                Mock.Of<IDotNetProjectBuilder>(),
                Mock.Of<IJavascriptMinifier>(),
                Mock.Of<ICssMinifier>(),
                Mock.Of<IScriptBundler>(),
                Mock.Of<IStyleBundler>(),
                Mock.Of<IConfigReader>(),
                Mock.Of<ICliVersionService>());

            var directory = "test-directory";
            var forceBuild = true;
            var projectType = "WebAssembly";

            // Act
            await bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
