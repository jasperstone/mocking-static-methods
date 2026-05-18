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
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenMonoMSBuildInstallationIsNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "projectFilePath", CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkBuildHostIsNotUsable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);
            var buildHostMock = new Mock<RemoteBuildHost>();
            buildHostMock.Setup(buildHost => buildHost.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "projectFilePath", CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
        }
    }
}
