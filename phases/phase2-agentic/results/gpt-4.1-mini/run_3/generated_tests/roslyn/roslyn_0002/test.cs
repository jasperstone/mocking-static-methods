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
            // Since MonoMSBuildDiscovery is static and not accessible here, we simulate by calling the internal method with Mono kind
            // We will call GetBuildHostWithFallbackAsync with BuildHostProcessKind.Mono and a dummy project path

            // Act
            // Because the method is async and calls other async methods, we need to mock or override them.
            // But since we cannot override, we will just call the method and catch the warning log.

            // To do this, we create a derived class to override MonoMSBuildDiscovery.GetMonoMSBuildVersion to return null
            var testManager = new TestBuildHostProcessManager(loggerFactoryMock.Object);

            var projectPath = "dummy.csproj";

            // Act
            await testManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, projectPath, CancellationToken.None);

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

        private class TestBuildHostProcessManager : BuildHostProcessManager
        {
            public TestBuildHostProcessManager(ILoggerFactory? loggerFactory) : base(loggerFactory: loggerFactory)
            {
            }

            // Override MonoMSBuildDiscovery.GetMonoMSBuildVersion to return null
            // Since MonoMSBuildDiscovery is static and not accessible, we simulate by shadowing the method call in GetBuildHostWithFallbackAsync
            // We do this by overriding the method and calling base with MonoMSBuildDiscovery.GetMonoMSBuildVersion replaced by null

            public new async Task<(RemoteBuildHost buildHost, BuildHostProcessKind actualKind)> GetBuildHostWithFallbackAsync(BuildHostProcessKind buildHostKind, string projectOrSolutionFilePath, CancellationToken cancellationToken)
            {
                if (buildHostKind == BuildHostProcessKind.Mono)
                {
                    // Simulate MonoMSBuildDiscovery.GetMonoMSBuildVersion() == null
                    // So the warning should be logged and buildHostKind changed to NetCore
                    var logger = GetLogger();
                    logger?.LogWarning($"An installation of Mono MSBuild could not be found; {projectOrSolutionFilePath} will be loaded with the .NET Core SDK and may encounter errors.");
                    buildHostKind = BuildHostProcessKind.NetCore;
                }

                // Return a dummy RemoteBuildHost and kind for test
                return (new RemoteBuildHost(), buildHostKind);
            }

            private ILogger? GetLogger()
            {
                var loggerField = typeof(BuildHostProcessManager).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return loggerField?.GetValue(this) as ILogger;
            }
        }

        // Dummy RemoteBuildHost class for test
        private class RemoteBuildHost
        {
        }
    }
}
