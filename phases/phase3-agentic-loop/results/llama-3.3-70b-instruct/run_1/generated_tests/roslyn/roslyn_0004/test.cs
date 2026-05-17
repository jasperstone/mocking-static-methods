using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Microsoft.CodeAnalysis.MSBuild
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_BuildHostProcess_Reloads()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)));

            // Act
            await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, "project.csproj", null, default);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString().Contains("reloading to start from")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
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
