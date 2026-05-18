using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BuildHostProcessManagerTests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_BuildHostProcess_Reloads()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)));

            var processPath = Path.GetFullPath(typeof(BuildHostProcessManager).Assembly.Location);
            var dotnetPath = Path.Combine(processPath, "..", "..", "dotnet");

            // Act
            await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, "project.csproj", dotnetPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)(v, t) => true), Times.Once);
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
