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
            _loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(_loggerMock.Object);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_MonoWithoutMSBuildLogsWarningAndFallsBack()
        {
            // Arrange
            var manager = new Mock<BuildHostProcessManager>(null, null, _loggerFactoryMock.Object);
            var projectPath = "someProject.csproj";

            // Simulate MonoMSBuildDiscovery.GetMonoMSBuildVersion() returns null
            // Since static method, assume it returns null

            // Act
            var result = await manager.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Mono MSBuild"))), Times.Once);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_NetFrameworkNotUsableLogsWarningAndFallsBack()
        {
            // Arrange
            var manager = new Mock<BuildHostProcessManager>(null, null, _loggerFactoryMock.Object);
            var projectPath = "someProject.csproj";

            // Mock a RemoteBuildHost with HasUsableMSBuildAsync returning false
            var mockBuildHost = new Mock<RemoteBuildHost>();
            mockBuildHost.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Setup GetBuildHostAsync to return our mock
            var mockManager = new Mock<BuildHostProcessManager>(null, null, _loggerFactoryMock.Object);
            mockManager.Setup(m => m.GetBuildHostAsync(It.IsAny<BuildHostProcessKind>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBuildHost.Object);

            // Act
            var result = await mockManager.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Visual Studio or the Build Tools"))), Times.Once);
            Assert.Equal(BuildHostProcessKind.NetCore, result.actualKind);
        }
    }
}
