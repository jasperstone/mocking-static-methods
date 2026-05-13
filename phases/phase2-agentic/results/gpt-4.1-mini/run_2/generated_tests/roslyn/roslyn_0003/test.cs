using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkBuildHostIsNotUsable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var buildHostMock = new Mock<RemoteBuildHost>();
            buildHostMock.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

            var buildHostProcessManagerMock = new Mock<BuildHostProcessManager>(null, null, loggerFactoryMock.Object)
            {
                CallBase = true
            };

            // Setup GetBuildHostAsync to return the mocked buildHostMock for NetFramework and NetCore
            buildHostProcessManagerMock
                .Setup(m => m.GetBuildHostAsync(BuildHostProcessKind.NetFramework, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildHostMock.Object);

            var fallbackBuildHostMock = new Mock<RemoteBuildHost>();
            buildHostProcessManagerMock
                .Setup(m => m.GetBuildHostAsync(BuildHostProcessKind.NetCore, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fallbackBuildHostMock.Object);

            var projectPath = "project.csproj";

            // Act
            var (resultBuildHost, actualKind) = await buildHostProcessManagerMock.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An installation of Visual Studio or the Build Tools for Visual Studio could not be found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal(fallbackBuildHostMock.Object, resultBuildHost);
            Assert.Equal(BuildHostProcessKind.NetCore, actualKind);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_DoesNotLogWarning_WhenNetFrameworkBuildHostIsUsable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var buildHostMock = new Mock<RemoteBuildHost>();
            buildHostMock.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);

            var buildHostProcessManagerMock = new Mock<BuildHostProcessManager>(null, null, loggerFactoryMock.Object)
            {
                CallBase = true
            };

            buildHostProcessManagerMock
                .Setup(m => m.GetBuildHostAsync(BuildHostProcessKind.NetFramework, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildHostMock.Object);

            var projectPath = "project.csproj";

            // Act
            var (resultBuildHost, actualKind) = await buildHostProcessManagerMock.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.Equal(buildHostMock.Object, resultBuildHost);
            Assert.Equal(BuildHostProcessKind.NetFramework, actualKind);
        }
    }
}
