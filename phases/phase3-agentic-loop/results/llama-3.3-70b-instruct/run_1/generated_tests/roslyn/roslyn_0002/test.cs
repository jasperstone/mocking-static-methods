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
        public async Task GetBuildHostWithFallbackAsync_LogsWarningWhenMonoMSBuildIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "project.csproj", CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarningWhenNetFrameworkMSBuildIsNotUsable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);
            var buildHostMock = new Mock<RemoteBuildHost>();
            buildHostMock.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "project.csproj", CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }
    }
}
