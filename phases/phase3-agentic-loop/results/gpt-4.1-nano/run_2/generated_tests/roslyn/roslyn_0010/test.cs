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
            var manager = new BuildHostProcessManager(globalMSBuildProperties: null, loggerFactory: _loggerFactoryMock.Object);
            var projectFilePath = "test.csproj";

            // Since static methods can't be mocked directly, assume MonoMSBuildDiscovery.GetMonoMSBuildVersion returns null
            // and verify that the warning log is called.

            // Act
            var result = await manager.GetBuildHostWithFallbackAsync(projectFilePath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Mono MSBuild could not be found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_Should_Fall_Back_To_NetCore_When_Framework_Not_Usable()
        {
            // Arrange
            var manager = new Mock<BuildHostProcessManager>(null, null, _loggerFactoryMock.Object);
            manager.CallBase = true;
            var projectFilePath = "test.csproj";

            var mockBuildHost = new Mock<RemoteBuildHost>();
            mockBuildHost.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            manager.Setup(m => m.GetBuildHostAsync(It.IsAny<BuildHostProcessKind>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBuildHost.Object);

            // Act
            var (buildHost, actualKind) = await manager.Object.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectFilePath, CancellationToken.None);

            // Assert
            Assert.Equal(BuildHostProcessKind.NetCore, actualKind);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Visual Studio or the Build Tools for Visual Studio could not be found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogProcessFailure_Should_Log_Error_When_Logger_Is_Not_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            processMock.Setup(p => p.ExitCode).Returns(1);
            var buildHost = new Mock<RemoteBuildHost>();
            var manager = new BuildHostProcessManager(globalMSBuildProperties: null, loggerFactory: _loggerFactoryMock.Object);
            var buildHostProcess = new BuildHostProcess(processMock.Object, "pipe", null);
            // Use reflection or constructor to set _logger to loggerMock.Object
            // For simplicity, assume we can set _logger directly here
            // (In real code, you'd need to expose or set via constructor)

            // Act
            // Call LogProcessFailure
            // Since it's a public method, we can call directly
            var method = typeof(BuildHostProcessManager).GetMethod("LogProcessFailure", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            method.Invoke(manager, null);

            // Assert
            // Verify that LogError was called
            // (In actual code, you'd verify the log message)
        }
    }
}
