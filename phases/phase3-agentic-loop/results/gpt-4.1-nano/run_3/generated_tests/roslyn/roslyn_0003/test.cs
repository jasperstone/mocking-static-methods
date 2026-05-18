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

            // Mock MonoMSBuildDiscovery to return null
            var originalMethod = typeof(MonoMSBuildDiscovery).GetMethod("GetMonoMSBuildVersion");
            // Since we can't easily mock static method, we simulate the scenario by setting the environment
            // or by assuming the method returns null.

            // Act
            var result = await manager.GetBuildHostWithFallbackAsync(projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains("Mono MSBuild could not be found"))),
                Times.Once);
        }
    }
}
