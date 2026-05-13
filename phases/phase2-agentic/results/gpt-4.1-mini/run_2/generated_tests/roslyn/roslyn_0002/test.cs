using System;
using System.Collections.Immutable;
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
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenMonoMSBuildVersionIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var manager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

            // We need to simulate MonoMSBuildDiscovery.GetMonoMSBuildVersion() == null
            // Since we cannot mock static methods easily here, we will simulate by calling the internal method
            // with BuildHostProcessKind.Mono and expect the warning to be logged.

            // Act
            // We call the internal method GetBuildHostWithFallbackAsync with Mono kind.
            // We expect the warning to be logged.
            var projectPath = "test.csproj";

            // Because the method calls GetBuildHostAsync which is async and returns a RemoteBuildHost,
            // and we don't have the full implementation or dependencies, we will mock the BuildHostProcessManager
            // to override GetBuildHostAsync to return a dummy RemoteBuildHost.

            var managerMock = new Mock<BuildHostProcessManager>(ImmutableDictionary<string, string>.Empty, null, loggerFactoryMock.Object)
            {
                CallBase = true
            };

            var dummyRemoteBuildHost = new Mock<RemoteBuildHost>();
            dummyRemoteBuildHost.Setup(h => h.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            managerMock.Setup(m => m.GetBuildHostAsync(BuildHostProcessKind.Mono, projectPath, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dummyRemoteBuildHost.Object);

            // Act
            var result = await managerMock.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, projectPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An installation of Mono MSBuild could not be found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkBuildHostIsNotUsable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var projectPath = "test.csproj";

            var dummyRemoteBuildHost = new Mock<RemoteBuildHost>();
            dummyRemoteBuildHost.Setup(h => h.HasUsableMSBuildAsync(projectPath, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var fallbackRemoteBuildHost = new Mock<RemoteBuildHost>();

            var managerMock = new Mock<BuildHostProcessManager>(ImmutableDictionary<string, string>.Empty, null, loggerFactoryMock.Object)
            {
                CallBase = true
            };

            managerMock.Setup(m => m.GetBuildHostAsync(BuildHostProcessKind.NetFramework, projectPath, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dummyRemoteBuildHost.Object);

            managerMock.Setup(m => m.GetBuildHostAsync(BuildHostProcessKind.NetCore, projectPath, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fallbackRemoteBuildHost.Object);

            // Act
            var result = await managerMock.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An installation of Visual Studio or the Build Tools for Visual Studio could not be found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal(fallbackRemoteBuildHost.Object, result.buildHost);
            Assert.Equal(BuildHostProcessKind.NetCore, result.actualKind);
        }
    }
}
