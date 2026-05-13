using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenMonoMSBuildNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new Mock<ILoggerFactory>().Object)
            {
                _logger = loggerMock.Object
            };

            // Mock MonoMSBuildDiscovery to return null
            var monoMSBuildDiscoveryMock = new Mock<IMonoMSBuildDiscovery>();
            monoMSBuildDiscoveryMock.Setup(m => m.GetMonoMSBuildVersion()).Returns((string)null);
            buildHostProcessManager._monoMSBuildDiscovery = monoMSBuildDiscoveryMock.Object;

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync("testProject.csproj", CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("An installation of Mono MSBuild could not be found"))),
                Times.Once);
        }
    }
}
