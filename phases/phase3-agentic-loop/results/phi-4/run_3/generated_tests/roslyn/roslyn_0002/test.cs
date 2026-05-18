using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenMonoMSBuildVersionIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var monoMSBuildDiscoveryMock = new Mock<IMonoMSBuildDiscovery>();
            monoMSBuildDiscoveryMock.Setup(m => m.GetMonoMSBuildVersion()).Returns((string)null);

            var buildHostProcessManager = new BuildHostProcessManager(
                globalMSBuildProperties: ImmutableDictionary<string, string>.Empty,
                binaryLogPathProvider: null,
                loggerFactory: loggerFactoryMock.Object,
                monoMSBuildDiscovery: monoMSBuildDiscoveryMock.Object);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync("test.csproj", CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("An installation of Mono MSBuild could not be found"))),
                Times.Once);
        }
    }

    internal interface IMonoMSBuildDiscovery
    {
        string? GetMonoMSBuildVersion();
    }

    internal class BuildHostProcessManager
    {
        private readonly ImmutableDictionary<string, string> _globalMSBuildProperties;
        private readonly IBinLogPathProvider? _binaryLogPathProvider;
        private readonly ILoggerFactory? _loggerFactory;
        private readonly ILogger? _logger;
        private readonly IMonoMSBuildDiscovery _monoMSBuildDiscovery;

        public BuildHostProcessManager(ImmutableDictionary<string, string>? globalMSBuildProperties = null, IBinLogPathProvider? binaryLogPathProvider = null, ILoggerFactory? loggerFactory = null, IMonoMSBuildDiscovery monoMSBuildDiscovery = null)
        {
            _globalMSBuildProperties = globalMSBuildProperties ?? ImmutableDictionary<string, string>.Empty;
            _binaryLogPathProvider = binaryLogPathProvider;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger<BuildHostProcessManager>();
            _monoMSBuildDiscovery = monoMSBuildDiscovery ?? new MonoMSBuildDiscovery();
        }

        public async Task<RemoteBuildHost> GetBuildHostWithFallbackAsync(string projectFilePath, CancellationToken cancellationToken)
        {
            var (buildHost, _) = await GetBuildHostWithFallbackAsync(GetKindForProject(projectFilePath), projectFilePath, cancellationToken).ConfigureAwait(false);
            return buildHost;
        }

        private async Task<(RemoteBuildHost buildHost, BuildHostProcessKind actualKind)> GetBuildHostWithFallbackAsync(BuildHostProcessKind buildHostKind, string projectOrSolutionFilePath, CancellationToken cancellationToken)
        {
            if (buildHostKind == BuildHostProcessKind.Mono && _monoMSBuildDiscovery.GetMonoMSBuildVersion() == null)
            {
                _logger?.LogWarning($"An installation of Mono MSBuild could not be found; {projectOrSolutionFilePath} will be loaded with the .NET Core SDK and may encounter errors.");
                buildHostKind = BuildHostProcessKind.NetCore;
            }

            var buildHost = await GetBuildHostAsync(buildHostKind, projectOrSolutionFilePath, dotnetPath: null, cancellationToken).ConfigureAwait(false);
            return (buildHost, buildHostKind);
        }

        private Task<RemoteBuildHost> GetBuildHostAsync(BuildHostProcessKind buildHostKind, string? projectOrSolutionFilePath, string? dotnetPath, CancellationToken cancellationToken)
        {
            // Mock implementation for testing
            return Task.FromResult(new RemoteBuildHost());
        }

        private BuildHostProcessKind GetKindForProject(string projectFilePath)
        {
            // Mock implementation for testing
            return BuildHostProcessKind.Mono;
        }
    }

    internal class RemoteBuildHost { }

    internal enum BuildHostProcessKind
    {
        Mono,
        NetCore,
        NetFramework
    }

    internal class MonoMSBuildDiscovery : IMonoMSBuildDiscovery
    {
        public string? GetMonoMSBuildVersion()
        {
            return null; // Simulate no Mono MSBuild version found
        }
    }

    internal interface IBinLogPathProvider { }
}
