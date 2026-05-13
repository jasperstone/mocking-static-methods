using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.CodeAnalysis.MSBuild;

public class BuildHostProcessManagerTests
{
    [Fact]
    public async Task GetBuildHostWithFallbackAsync_LogsWarningWhenMonoMSBuildNotFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var monoMSBuildDiscoveryMock = new Mock<IMonoMSBuildDiscovery>();
        monoMSBuildDiscoveryMock.Setup(m => m.GetMonoMSBuildVersion()).Returns((string)null);

        var buildHostProcessManager = new BuildHostProcessManager(
            loggerFactory: new Mock<ILoggerFactory>().Object,
            globalMSBuildProperties: null,
            binaryLogPathProvider: null)
        {
            _logger = loggerMock.Object,
            MonoMSBuildDiscovery = monoMSBuildDiscoveryMock.Object
        };

        // Act
        await buildHostProcessManager.GetBuildHostWithFallbackAsync("test.csproj", CancellationToken.None);

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(It.Is<string>(s => s.Contains("An installation of Mono MSBuild could not be found"))),
            Times.Once);
    }
}
