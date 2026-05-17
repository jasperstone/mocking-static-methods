using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BuildHostProcessManagerTests
{
    [Fact]
    public async Task LogInformation_Called_When_Relaunching_BuildHost()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

        var buildHostProcessManager = new BuildHostProcessManager(
            globalMSBuildProperties: null,
            binaryLogPathProvider: null,
            loggerFactory: loggerFactoryMock.Object);

        var buildHostProcessMock = new Mock<BuildHostProcess>(null, null, null);
        var buildHostMock = new Mock<RemoteBuildHost>();
        buildHostProcessMock.Setup(b => b.BuildHost).Returns(buildHostMock.Object);

        var msbuildLocation = new MSBuildLocation { Path = "/path/to/msbuild" };
        buildHostMock.Setup(b => b.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(msbuildLocation);

        var processPath = "/path/to/process";
        var dotnetPath = "/path/to/dotnet";

        // Act
        await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, "/path/to/project", null, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
