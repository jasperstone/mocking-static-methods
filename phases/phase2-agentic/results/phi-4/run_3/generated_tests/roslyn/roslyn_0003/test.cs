using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BuildHostProcessManagerTests
{
    [Fact]
    public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkMSBuildNotAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var buildHostProcessManager = new BuildHostProcessManager(
            loggerFactory: new Mock<ILoggerFactory>().Object)
        {
            _logger = loggerMock.Object
        };

        var buildHostMock = new Mock<RemoteBuildHost>();
        buildHostMock
            .Setup(bh => bh.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var buildHostProcessMock = new Mock<BuildHostProcess>();
        buildHostProcessMock
            .Setup(bp => bp.BuildHost)
            .Returns(buildHostMock.Object);

        buildHostProcessManager._processes = new Dictionary<BuildHostProcessKind, BuildHostProcess>
        {
            { BuildHostProcessKind.NetFramework, buildHostProcessMock.Object }
        };

        // Act
        await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, "test.csproj", CancellationToken.None);

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(It.Is<string>(s => s.Contains("An installation of Visual Studio or the Build Tools for Visual Studio could not be found"))),
            Times.Once);
    }
}
