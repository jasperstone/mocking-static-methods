using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.Bundling;

namespace Volo.Abp.Cli.Tests.Bundling
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task LogInformation_ShouldBeCalled_WhenGeneratingScriptReferences()
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

            // Act
            var scriptContext = bundlingService.GetScriptContext(bundleDefinitions, bundleConfig, "WebAssembly");

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Generating script references..."),
                Times.Once);
        }
    }
}
