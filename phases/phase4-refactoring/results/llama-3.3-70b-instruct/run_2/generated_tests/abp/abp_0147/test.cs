using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Bundling;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_StyleBundleGeneratedSuccessfully_LoggerInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService(
                loggerMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            await bundlingService.BundleAsync("directory", false, "WebAssembly");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Generating style bundle..."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Style bundle has been generated successfully."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_ScriptBundleGeneratedSuccessfully_LoggerInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService(
                loggerMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            await bundlingService.BundleAsync("directory", false, "WebAssembly");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Generating script bundle..."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Script bundle has been generated successfully."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_StyleReferencesGeneratedSuccessfully_LoggerInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService(
                loggerMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            await bundlingService.BundleAsync("directory", false, "WebAssembly");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Generating style references..."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_ScriptReferencesGeneratedSuccessfully_LoggerInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService(
                loggerMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            await bundlingService.BundleAsync("directory", false, "WebAssembly");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Generating script references..."), Times.Once);
        }
    }
}
