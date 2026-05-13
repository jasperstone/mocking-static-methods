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

            var buildHostProcessManager = new TestBuildHostProcessManager(loggerFactoryMock.Object, buildHostMock.Object);

            var projectPath = "project.csproj";

            // Act
            var (buildHost, actualKind) = await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An installation of Visual Studio or the Build Tools for Visual Studio could not be found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal(BuildHostProcessKind.NetCore, actualKind);
            Assert.Same(buildHostMock.Object, buildHost);
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

            var buildHostProcessManager = new TestBuildHostProcessManager(loggerFactoryMock.Object, buildHostMock.Object);

            var projectPath = "project.csproj";

            // Act
            var (buildHost, actualKind) = await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.Equal(BuildHostProcessKind.NetFramework, actualKind);
            Assert.Same(buildHostMock.Object, buildHost);
        }

        private class TestBuildHostProcessManager : BuildHostProcessManager
        {
            private readonly RemoteBuildHost _buildHostToReturn;

            public TestBuildHostProcessManager(ILoggerFactory loggerFactory, RemoteBuildHost buildHostToReturn)
                : base(loggerFactory: loggerFactory)
            {
                _buildHostToReturn = buildHostToReturn;
            }

            public override Task<RemoteBuildHost> GetBuildHostAsync(BuildHostProcessKind buildHostKind, string? projectOrSolutionFilePath, string? dotnetPath, CancellationToken cancellationToken)
            {
                return Task.FromResult(_buildHostToReturn);
            }
        }
    }
}
