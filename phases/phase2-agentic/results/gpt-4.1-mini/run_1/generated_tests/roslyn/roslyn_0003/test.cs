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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var buildHostMock = new Mock<RemoteBuildHost>();
            buildHostMock.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

            var manager = new TestBuildHostProcessManager(loggerFactoryMock.Object, buildHostMock.Object);

            var projectPath = "project.csproj";

            // Act
            var (buildHost, kind) = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An installation of Visual Studio or the Build Tools for Visual Studio could not be found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal(BuildHostProcessKind.NetCore, kind);
            Assert.Same(manager.NetCoreBuildHost, buildHost);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_DoesNotLogWarning_WhenNetFrameworkBuildHostIsUsable()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var buildHostMock = new Mock<RemoteBuildHost>();
            buildHostMock.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);

            var manager = new TestBuildHostProcessManager(loggerFactoryMock.Object, buildHostMock.Object);

            var projectPath = "project.csproj";

            // Act
            var (buildHost, kind) = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.Equal(BuildHostProcessKind.NetFramework, kind);
            Assert.Same(buildHostMock.Object, buildHost);
        }

        private class TestBuildHostProcessManager : BuildHostProcessManager
        {
            private readonly RemoteBuildHost _netFrameworkBuildHost;
            public RemoteBuildHost NetCoreBuildHost { get; }

            public TestBuildHostProcessManager(ILoggerFactory loggerFactory, RemoteBuildHost netFrameworkBuildHost)
                : base(loggerFactory: loggerFactory)
            {
                _netFrameworkBuildHost = netFrameworkBuildHost;
                NetCoreBuildHost = new Mock<RemoteBuildHost>().Object;
            }

            public override Task<RemoteBuildHost> GetBuildHostAsync(BuildHostProcessKind buildHostKind, string? projectOrSolutionFilePath, string? dotnetPath, CancellationToken cancellationToken)
            {
                if (buildHostKind == BuildHostProcessKind.NetFramework)
                    return Task.FromResult(_netFrameworkBuildHost);
                else if (buildHostKind == BuildHostProcessKind.NetCore)
                    return Task.FromResult(NetCoreBuildHost);

                return base.GetBuildHostAsync(buildHostKind, projectOrSolutionFilePath, dotnetPath, cancellationToken);
            }
        }
    }
}
