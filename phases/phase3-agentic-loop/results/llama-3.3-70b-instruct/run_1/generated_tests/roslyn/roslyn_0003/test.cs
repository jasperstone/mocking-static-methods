using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BuildHostProcessManagerTests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenMonoMSBuildIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactory);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "projectFilePath", CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkBuildHostIsNotUsable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactory);

            // Act
            await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "projectFilePath", CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
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
