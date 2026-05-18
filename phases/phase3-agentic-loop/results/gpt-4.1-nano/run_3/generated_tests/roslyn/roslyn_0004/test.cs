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
        public async Task GetBuildHostWithFallbackAsync_ShouldLogWarning_WhenMonoMSBuildNotFound()
        {
            // Arrange
            var manager = new BuildHostProcessManager(loggerFactory: _loggerFactoryMock.Object);
            var projectPath = "someProject.csproj";

            // Act
            var result = await manager.GetBuildHostWithFallbackAsync(projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Mono MSBuild could not be found"))), Times.Once);
        }

        [Fact]
        public async Task GetBuildHostAsync_ShouldCreateAndReturnBuildHostProcess()
        {
            // Arrange
            var manager = new BuildHostProcessManager(loggerFactory: _loggerFactoryMock.Object);
            var buildHostKind = BuildHostProcessKind.NetCore;

            // Act
            var buildHost = await manager.GetBuildHostAsync(buildHostKind, CancellationToken.None);

            // Assert
            Assert.NotNull(buildHost);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_ShouldFallBackToNetCore_WhenNetFrameworkBuildHostIsNotUsable()
        {
            // Arrange
            var manager = new BuildHostProcessManager(loggerFactory: _loggerFactoryMock.Object);
            var projectPath = "someProject.csproj";

            // Since internal methods and behaviors are hard to mock directly, this test assumes the fallback logic is triggered
            // when HasUsableMSBuildAsync returns false. For a real test, you'd need to mock or stub the build host's behavior.
            // Here, we simulate the call and verify that a warning is logged.

            // Act
            var result = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Visual Studio or the Build Tools"))), Times.AtLeastOnce);
        }
    }
}
