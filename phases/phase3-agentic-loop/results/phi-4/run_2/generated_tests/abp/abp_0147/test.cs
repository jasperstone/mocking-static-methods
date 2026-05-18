using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.Bundling;

namespace Volo.Abp.Cli.Tests
{
    // Mock or simple implementation of BundleTypeDefinition
    public class MockBundleTypeDefinition
    {
        public string BundleContributorType { get; set; }
    }

    public class BundlingServiceTests
    {
        [Fact]
        public async Task LogInformation_ShouldBeCalled_WhenGeneratingScriptReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var projectBuilderMock = new Mock<IDotNetProjectBuilder>();

            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object,
                DotNetProjectBuilder = projectBuilderMock.Object
            };

            var bundleDefinitions = new List<MockBundleTypeDefinition>
            {
                new MockBundleTypeDefinition { BundleContributorType = "SomeType" }
            };

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                InteractiveAuto = false
            };

            // Mock the file system interactions
            projectBuilderMock
                .Setup(pb => pb.BuildProjects(It.IsAny<List<DotNetProjectInfo>>(), It.IsAny<string>()))
                .Verifiable();

            // Act
            await bundlingService.BundleAsync("testDirectory", false, "WebAssembly");

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Generating script references..."),
                Times.Once);

            projectBuilderMock.Verify();
        }
    }
}
