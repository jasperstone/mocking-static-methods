using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

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
        await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, "project.csproj", CancellationToken.None);

        // Assert
        loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkBuildHostIsNotUsable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
        var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactory);
        var buildHostMock = new Mock<RemoteBuildHost>();
        buildHostMock.Setup(buildHost => buildHost.HasUsableMSBuildAsync("project.csproj", CancellationToken.None)).ReturnsAsync(false);

        // Act
        await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "project.csproj", CancellationToken.None);

        // Assert
        loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
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
