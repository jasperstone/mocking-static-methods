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
    public async Task LogInformation_Called_With_Correct_Parameters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

        var buildHostProcessManager = new BuildHostProcessManager(
            globalMSBuildProperties: ImmutableDictionary<string, string>.Empty,
            binaryLogPathProvider: null,
            loggerFactory: loggerFactoryMock.Object);

        var buildHostProcessMock = new Mock<BuildHostProcess>(null, null, null);
        var buildHostMock = new Mock<RemoteBuildHost>();
        buildHostProcessMock.Setup(b => b.BuildHost).Returns(buildHostMock.Object);

        var msbuildLocationMock = new Mock<MSBuildLocation>();
        msbuildLocationMock.Setup(m => m.Path).Returns("/path/to/msbuild");

        buildHostMock.Setup(b => b.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(msbuildLocationMock.Object);

        var processPath = "/path/to/process";
        var dotnetPath = "/path/to/dotnet";

        // Mock the GetProcessPath method
        var buildHostProcessManagerMock = new Mock<BuildHostProcessManager>(ImmutableDictionary<string, string>.Empty, null, loggerFactoryMock.Object);
        buildHostProcessManagerMock.Setup(m => m.GetProcessPath()).Returns(processPath);

        // Act
        await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, "/path/to/project", dotnetPath, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                ".NET BuildHost started from {ProcessPath} reloading to start from {DotnetPath} to match necessary SDK location.",
                processPath,
                dotnetPath),
            Times.Once);
    }
}
