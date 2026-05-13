using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.MSBuild
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarningWhenMonoMSBuildIsNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "projectFilePath", CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarningWhenNetFrameworkMSBuildIsNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "projectFilePath", CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
        }
    }
}
