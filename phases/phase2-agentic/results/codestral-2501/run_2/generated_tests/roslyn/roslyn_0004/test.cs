using System;
using System.Collections.Generic;
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
    public async Task LogInformation_Called_When_DotnetPath_Differs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var buildHostProcessMock = new Mock<BuildHostProcess>(null, null, null);
        var remoteBuildHostMock = new Mock<RemoteBuildHost>();
        var msbuildLocationMock = new Mock<MSBuildLocation>();

        var projectOrSolutionFilePath = "path/to/project";
        var cancellationToken = CancellationToken.None;
        var processPath = "path/to/process";
        var dotnetPath = "path/to/dotnet";

        buildHostProcessMock.Setup(b => b.BuildHost).Returns(remoteBuildHostMock.Object);
        remoteBuildHostMock.Setup(r => r.FindBestMSBuildAsync(projectOrSolutionFilePath, cancellationToken)).ReturnsAsync(msbuildLocationMock.Object);
        msbuildLocationMock.Setup(m => m.Path).Returns("path/to/msbuild");

        var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);

        // Act
        await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, projectOrSolutionFilePath, dotnetPath, cancellationToken);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                ".NET BuildHost started from {ProcessPath} reloading to start from {DotnetPath} to match necessary SDK location.",
                processPath,
                dotnetPath),
            Times.Once);
    }
}
