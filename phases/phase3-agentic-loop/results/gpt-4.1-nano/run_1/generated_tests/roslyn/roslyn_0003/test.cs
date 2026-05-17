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
            var manager = new TestBuildHostProcessManager(_loggerFactoryMock.Object);
            var projectPath = "test.csproj";

            // Act
            var result = await manager.GetBuildHostWithFallbackAsync(projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(msg => msg.Contains("Mono MSBuild could not be found"))),
                Times.Once);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_ShouldFallBackToNetCore_WhenNetFrameworkBuildHostIsNotUsable()
        {
            // Arrange
            var manager = new TestBuildHostProcessManager(_loggerFactoryMock.Object);
            var projectPath = "test.csproj";

            // Setup to simulate NetFramework build host that is not usable
            manager.SetupBuildHostKind(BuildHostProcessKind.NetFramework);
            manager.SetupBuildHostAsync(BuildHostProcessKind.NetFramework, new FakeBuildHost(false));
            manager.SetupBuildHostAsync(BuildHostProcessKind.NetCore, new FakeBuildHost(true));

            // Act
            var (buildHost, actualKind) = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            Assert.Equal(BuildHostProcessKind.NetCore, actualKind);
            Assert.IsType<FakeBuildHost>(buildHost);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_ShouldNotLogWarning_WhenMonoMSBuildFound()
        {
            // Arrange
            var manager = new TestBuildHostProcessManager(_loggerFactoryMock.Object);
            var projectPath = "test.csproj";

            // Setup to simulate Mono MSBuild version found
            manager.SetupMonoMSBuildVersion("someVersion");

            // Act
            var result = await manager.GetBuildHostWithFallbackAsync(projectPath, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(msg => msg.Contains("Mono MSBuild could not be found"))),
                Times.Never);
        }

        // Helper classes for testing
        private class FakeBuildHost : RemoteBuildHost
        {
            private readonly bool _isUsable;

            public FakeBuildHost(bool isUsable)
            {
                _isUsable = isUsable;
            }

            public override Task<bool> HasUsableMSBuildAsync(string projectFilePath, CancellationToken cancellationToken)
            {
                return Task.FromResult(_isUsable);
            }
        }

        private class TestBuildHostProcessManager : BuildHostProcessManager
        {
            private BuildHostProcessKind _buildHostKind;
            private readonly Mock<BuildHostProcess> _buildHostMock = new Mock<BuildHostProcess>();
            private string _monoMSBuildVersion;

            public TestBuildHostProcessManager(ILoggerFactory loggerFactory)
                : base(null, null, loggerFactory)
            {
                _buildHostMock.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
            }

            public void SetupBuildHostKind(BuildHostProcessKind kind)
            {
                _buildHostKind = kind;
            }

            public void SetupBuildHostAsync(BuildHostProcessKind kind, RemoteBuildHost buildHost)
            {
                if (kind == _buildHostKind)
                {
                    _buildHostMock.Setup(b => b.BuildHost).Returns(buildHost);
                }
            }

            public void SetupMonoMSBuildVersion(string version)
            {
                _monoMSBuildVersion = version;
            }

            public override async Task<(RemoteBuildHost buildHost, BuildHostProcessKind actualKind)> GetBuildHostWithFallbackAsync(BuildHostProcessKind buildHostKind, string projectFilePath, CancellationToken cancellationToken)
            {
                // Simulate Mono MSBuild version check
                if (buildHostKind == BuildHostProcessKind.Mono && _monoMSBuildVersion == null)
                {
                    _logger?.LogWarning($"An installation of Mono MSBuild could not be found; {projectFilePath} will be loaded with the .NET Core SDK and may encounter errors.");
                    buildHostKind = BuildHostProcessKind.NetCore;
                }
                return await base.GetBuildHostWithFallbackAsync(buildHostKind, projectFilePath, cancellationToken);
            }

            public override Task<RemoteBuildHost> GetBuildHostAsync(BuildHostProcessKind buildHostKind, string? projectOrSolutionFilePath, string? dotnetPath, CancellationToken cancellationToken)
            {
                return Task.FromResult(_buildHostMock.Object);
            }
        }
    }
}
