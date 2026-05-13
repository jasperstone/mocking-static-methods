using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;
using Volo.Abp.Cli.Bundling;

namespace Volo.Abp.Cli.Tests.Bundling
{
    public class BundlingServiceTests
    {
        [Fact]
        public void Should_LogInformation_When_GeneratingScriptReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object
            };

            var bundleDefinitions = new List<BundleTypeDefinition>();
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                InteractiveAuto = false
            };

            var projectType = "SomeProjectType";

            // Act
            bundlingService.GetScriptContext(bundleDefinitions, bundleConfig, projectType);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Generating script references..."),
                Times.Once);
        }
    }
}
