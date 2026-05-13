using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_StyleBundleGeneratedSuccessfully_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService(
                loggerMock.Object,
                new Mock<IDotNetProjectBuilder>().Object,
                new Mock<IJavascriptMinifier>().Object,
                new Mock<ICssMinifier>().Object,
                new Mock<IScriptBundler>().Object,
                new Mock<IStyleBundler>().Object,
                new Mock<IConfigReader>().Object,
                new Mock<CliVersionService>().Object
            );

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Bundle,
                Name = "TestBundle"
            };

            // Act
            await bundlingService.BundleAsync("TestDirectory", false, BundlingConsts.WebAssembly);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Generating style bundle..."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Style bundle has been generated successfully."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_ScriptBundleGeneratedSuccessfully_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService(
                loggerMock.Object,
                new Mock<IDotNetProjectBuilder>().Object,
                new Mock<IJavascriptMinifier>().Object,
                new Mock<ICssMinifier>().Object,
                new Mock<IScriptBundler>().Object,
                new Mock<IStyleBundler>().Object,
                new Mock<IConfigReader>().Object,
                new Mock<CliVersionService>().Object
            );

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Bundle,
                Name = "TestBundle"
            };

            // Act
            await bundlingService.BundleAsync("TestDirectory", false, BundlingConsts.WebAssembly);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Generating script bundle..."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Script bundle has been generated successfully."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_StyleReferencesGeneratedSuccessfully_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService(
                loggerMock.Object,
                new Mock<IDotNetProjectBuilder>().Object,
                new Mock<IJavascriptMinifier>().Object,
                new Mock<ICssMinifier>().Object,
                new Mock<IScriptBundler>().Object,
                new Mock<IStyleBundler>().Object,
                new Mock<IConfigReader>().Object,
                new Mock<CliVersionService>().Object
            );

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Reference,
                Name = "TestBundle"
            };

            // Act
            await bundlingService.BundleAsync("TestDirectory", false, BundlingConsts.WebAssembly);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Generating style references..."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_ScriptReferencesGeneratedSuccessfully_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService(
                loggerMock.Object,
                new Mock<IDotNetProjectBuilder>().Object,
                new Mock<IJavascriptMinifier>().Object,
                new Mock<ICssMinifier>().Object,
                new Mock<IScriptBundler>().Object,
                new Mock<IStyleBundler>().Object,
                new Mock<IConfigReader>().Object,
                new Mock<CliVersionService>().Object
            );

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Reference,
                Name = "TestBundle"
            };

            // Act
            await bundlingService.BundleAsync("TestDirectory", false, BundlingConsts.WebAssembly);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Generating script references..."), Times.Once);
        }
    }
}
