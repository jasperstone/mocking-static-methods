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
    public async Task LogInformation_Called_When_DotnetPath_Different()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var buildHostProcessManager = new BuildHostProcessManager(
            globalMSBuildProperties: ImmutableDictionary<string, string>.Empty,
            binaryLogPathProvider: null,
            loggerFactory: loggerFactoryMock.Object);

        var buildHostProcessMock = new Mock<BuildHostProcess>(null, null, null);
        var remoteBuildHostMock = new Mock<RemoteBuildHost>();
        buildHostProcessMock.Setup(bhp => bhp.BuildHost).Returns(remoteBuildHostMock.Object);

        var msbuildLocation = new MSBuildLocation { Path = "/path/to/msbuild" };
        remoteBuildHostMock.Setup(rbh => rbh.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(msbuildLocation);

        var processPath = "/path/to/process";
        var dotnetPath = "/path/to/dotnet";

        // Act
        await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, "projectOrSolutionFilePath", dotnetPath, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
