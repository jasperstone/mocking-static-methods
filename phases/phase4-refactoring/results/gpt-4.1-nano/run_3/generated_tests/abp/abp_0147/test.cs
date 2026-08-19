using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Bundling;

namespace BundlingServiceTests
{
    public class BundlingServiceTest
    {
        [Fact]
        public async Task BundleAsync_Should_Log_GeneratingStyleReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object,
                // Set other dependencies or mock them as needed
            };

            // Setup parameters to ensure code reaches the branch with "Generating style references..."
            string directory = Path.GetTempPath();
            bool forceBuild = false;
            string projectType = "WebAssembly";

            // Mock dependencies or methods if necessary
            // For example, you might need to mock ConfigReader.Read, GetTargetFrameworkVersion, etc.
            // For simplicity, assume defaults or that the method can run without actual project files.

            // Act
            await bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            // Verify that LogInformation was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating style references...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
