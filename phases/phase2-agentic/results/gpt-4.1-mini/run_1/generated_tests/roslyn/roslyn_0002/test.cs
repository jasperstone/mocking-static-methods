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
        private enum BuildHostProcessKind
        {
            Mono,
            NetCore,
            NetFramework
        }

        private class RemoteBuildHost
        {
            public virtual Task<bool> HasUsableMSBuildAsync(string? projectOrSolutionFilePath, CancellationToken cancellationToken) => Task.FromResult(true);
        }

        private class BuildHostProcessManagerTestable : BuildHostProcessManager
        {
            private readonly Func<BuildHostProcessKind, string?, string?, CancellationToken, Task<RemoteBuildHost>> _getBuildHostAsyncOverride;
            private readonly Func<BuildHostProcessKind, string> _getMonoVersionOverride;

            public BuildHostProcessManagerTestable(
                Func<BuildHostProcessKind, string?, string?, CancellationToken, Task<RemoteBuildHost>> getBuildHostAsyncOverride,
                Func<BuildHostProcessKind, string> getMonoVersionOverride,
                ILoggerFactory? loggerFactory = null)
                : base(ImmutableDictionary<string, string>.Empty, null, loggerFactory)
            {
                _getBuildHostAsyncOverride = getBuildHostAsyncOverride;
                _getMonoVersionOverride = getMonoVersionOverride;
            }

            public new async Task<(RemoteBuildHost buildHost, BuildHostProcessKind actualKind)> GetBuildHostWithFallbackAsync(BuildHostProcessKind buildHostKind, string projectOrSolutionFilePath, CancellationToken cancellationToken)
            {
                if (buildHostKind == BuildHostProcessKind.Mono && _getMonoVersionOverride(buildHostKind) == null)
                {
                    Logger?.LogWarning($"An installation of Mono MSBuild could not be found; {projectOrSolutionFilePath} will be loaded with the .NET Core SDK and may encounter errors.");
                    buildHostKind = BuildHostProcessKind.NetCore;
                }

                var buildHost = await _getBuildHostAsyncOverride(buildHostKind, projectOrSolutionFilePath, null, cancellationToken).ConfigureAwait(false);

                if (buildHostKind == BuildHostProcessKind.NetFramework)
                {
                    if (!await buildHost.HasUsableMSBuildAsync(projectOrSolutionFilePath, cancellationToken).ConfigureAwait(false))
                    {
                        Logger?.LogWarning($"An installation of Visual Studio or the Build Tools for Visual Studio could not be found; {projectOrSolutionFilePath} will be loaded with the .NET Core SDK and may encounter errors.");
                        return (await _getBuildHostAsyncOverride(BuildHostProcessKind.NetCore, projectOrSolutionFilePath, null, cancellationToken).ConfigureAwait(false), BuildHostProcessKind.NetCore);
                    }
                }

                return (buildHost, buildHostKind);
            }

            public ILogger? Logger => typeof(BuildHostProcessManager)
                .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(this) as ILogger;
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenMonoMSBuildVersionIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var buildHost = new RemoteBuildHost();

            var manager = new BuildHostProcessManagerTestable(
                getBuildHostAsyncOverride: (kind, path, dotnetPath, token) => Task.FromResult(buildHost),
                getMonoVersionOverride: kind => null,
                loggerFactory: loggerFactoryMock.Object);

            var projectPath = "project.csproj";

            // Act
            var (result, actualKind) = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, projectPath, CancellationToken.None);

            // Assert
            Assert.Equal(buildHost, result);
            Assert.Equal(BuildHostProcessKind.NetCore, actualKind);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An installation of Mono MSBuild could not be found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkHasNoUsableMSBuild()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var unusableBuildHostMock = new Mock<RemoteBuildHost>();
            unusableBuildHostMock.Setup(b => b.HasUsableMSBuildAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var usableBuildHost = new RemoteBuildHost();

            var getBuildHostAsyncCalls = 0;
            var manager = new BuildHostProcessManagerTestable(
                getBuildHostAsyncOverride: (kind, path, dotnetPath, token) =>
                {
                    getBuildHostAsyncCalls++;
                    if (getBuildHostAsyncCalls == 1)
                        return Task.FromResult((RemoteBuildHost)unusableBuildHostMock.Object);
                    else
                        return Task.FromResult(usableBuildHost);
                },
                getMonoVersionOverride: kind => "some-version",
                loggerFactory: loggerFactoryMock.Object);

            var projectPath = "project.csproj";

            // Act
            var (result, actualKind) = await manager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectPath, CancellationToken.None);

            // Assert
            Assert.Equal(usableBuildHost, result);
            Assert.Equal(BuildHostProcessKind.NetCore, actualKind);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An installation of Visual Studio or the Build Tools for Visual Studio could not be found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
