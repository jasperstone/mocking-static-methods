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
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkHostIsNotUsable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)));

            var cancellationToken = CancellationToken.None;

            // Mock the behavior of the build host to simulate a non-usable .NET Framework host
            var buildHostMock = new Mock<RemoteBuildHost>();
            buildHostMock
                .Setup(bh => bh.HasUsableMSBuildAsync(It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(false);

            var getBuildHostAsyncMock = new Mock<Func<BuildHostProcessKind, string?, string?, CancellationToken, Task<RemoteBuildHost>>>();
            getBuildHostAsyncMock
                .Setup(g => g(BuildHostProcessKind.NetFramework, It.IsAny<string>(), null, cancellationToken))
                .ReturnsAsync(buildHostMock.Object);
            getBuildHostAsyncMock
                .Setup(g => g(BuildHostProcessKind.NetCore, It.IsAny<string>(), null, cancellationToken))
                .ReturnsAsync(new RemoteBuildHost());

            var originalMethod = typeof(BuildHostProcessManager).GetMethod("GetBuildHostAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            originalMethod = originalMethod.MakeGenericMethod(typeof(RemoteBuildHost));
            originalMethod.Invoke(buildHostProcessManager, new object[] { getBuildHostAsyncMock.Object });

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "dummyPath", cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("An installation of Visual Studio or the Build Tools for Visual Studio could not be found")), It.IsAny<object[]>()), Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }
}
