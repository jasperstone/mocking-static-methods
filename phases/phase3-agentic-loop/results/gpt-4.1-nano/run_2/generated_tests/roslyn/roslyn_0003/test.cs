using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class BuildHostProcessManagerTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _loggerMock;

        public BuildHostProcessManagerTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_ShouldLogWarning_WhenMonoMSBuildNotFound()
        {
            // Arrange
            var manager = new BuildHostProcessManager(loggerFactory: _loggerFactoryMock.Object);
            var projectPath = "test.csproj";

            // Simulate MonoMSBuildDiscovery.GetMonoMSBuildVersion() returns null
            // Since static methods can't be mocked directly, we simulate the behavior by setting buildHostKind to Mono
            var buildHostKind = BuildHostProcessKind.Mono;

            // Act
            var result = await manager.GetBuildHostWithFallbackAsync(buildHostKind, projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains("Mono MSBuild could not be found"))),
                Times.Once);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_ShouldLogWarning_WhenBuildHostIsNotUsable()
        {
            // Arrange
            var manager = new Mock<BuildHostProcessManager>(null, null, _loggerFactoryMock.Object);
            var projectPath = "test.csproj";

            // Mock buildHost to return false for HasUsableMSBuildAsync
            var mockBuildHost = new Mock<RemoteBuildHost>();
            mockBuildHost.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Setup GetBuildHostAsync to return our mock build host
            manager.Setup(m => m.GetBuildHostAsync(It.IsAny<BuildHostProcessKind>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBuildHost.Object);

            // Act
            var result = await manager.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains("Visual Studio or the Build Tools for Visual Studio could not be found"))),
                Times.Once);
        }
    }
}
