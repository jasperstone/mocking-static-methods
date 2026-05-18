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
        public async Task GetBuildHostWithFallbackAsync_Should_Log_Warning_When_Mono_Not_Found()
        {
            // Arrange
            var manager = new Mock<BuildHostProcessManager>(null, null, _loggerFactoryMock.Object) { CallBase = true };
            var projectFilePath = "test.csproj";

            // Simulate MonoMSBuildDiscovery.GetMonoMSBuildVersion returning null
            // Since it's static, assume environment where Mono is not available
            // and verify that a warning log is emitted

            // Act
            var result = await manager.Object.GetBuildHostWithFallbackAsync(projectFilePath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Mono MSBuild"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_Should_Fallback_To_NetCore_When_Framework_Not_Usable()
        {
            // Arrange
            var manager = new Mock<BuildHostProcessManager>(null, null, _loggerFactoryMock.Object) { CallBase = true };
            var projectFilePath = "test.csproj";

            var mockBuildHost = new Mock<RemoteBuildHost>();
            var mockBuildHostProcess = new Mock<BuildHostProcess>(MockBehavior.Strict, new Process(), "pipe", null);
            mockBuildHostProcess.Setup(b => b.HasUsableMSBuildAsync(projectFilePath, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            mockBuildHost.Setup(b => b.BuildHost).Returns(mockBuildHost.Object);

            // Setup GetBuildHostAsync to return the mocked build host
            manager.Setup(m => m.GetBuildHostAsync(It.IsAny<BuildHostProcessKind>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBuildHost.Object);

            // Act
            var result = await manager.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectFilePath, CancellationToken.None);

            // Assert
            Assert.Equal(BuildHostProcessKind.NetCore, result.actualKind);
            manager.Verify(m => m.GetBuildHostAsync(BuildHostProcessKind.NetCore, projectFilePath, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetBuildHostAsync_Should_Create_New_Process_When_Not_Exists()
        {
            // Arrange
            var manager = new Mock<BuildHostProcessManager>(null, null, _loggerFactoryMock.Object) { CallBase = true };
            var buildHostKind = BuildHostProcessKind.NetCore;

            // Setup to simulate process creation
            var dummyProcess = new Process();
            var mockBuildHost = new Mock<RemoteBuildHost>();
            var mockBuildHostProcess = new Mock<BuildHostProcess>(MockBehavior.Strict, dummyProcess, "pipe", null);
            mockBuildHostProcess.Setup(b => b.BuildHost).Returns(mockBuildHost.Object);
            mockBuildHostProcess.Setup(b => b.LogProcessFailure()).Verifiable();

            // Since Process.Start is static, assume the code is structured to allow injection or override
            // For this test, focus on the flow assuming process creation succeeds

            // Act
            var result = await manager.Object.GetBuildHostAsync(buildHostKind, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            // Additional assertions depend on actual implementation details
        }
    }
}
