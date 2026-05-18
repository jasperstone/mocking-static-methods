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
    public async Task GetBuildHostAsync_LogsInformation_WhenDotnetPathIsDifferent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

        var buildHostProcessMock = new Mock<BuildHostProcess>(null, null, null);
        buildHostProcessMock.Setup(b => b.BuildHost.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MSBuildLocation { Path = "path/to/msbuild" });

        var buildHostProcessManager = new BuildHostProcessManager(
            globalMSBuildProperties: null,
            binaryLogPathProvider: null,
            loggerFactory: loggerFactoryMock.Object);

        var projectOrSolutionFilePath = "path/to/project.csproj";
        var cancellationToken = CancellationToken.None;

        // Act
        await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, projectOrSolutionFilePath, null, cancellationToken);

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

    [Fact]
    public async Task GetBuildHostAsync_ReturnsBuildHost_WhenDotnetPathIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

        var buildHostProcessMock = new Mock<BuildHostProcess>(null, null, null);
        buildHostProcessMock.Setup(b => b.BuildHost.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MSBuildLocation)null);

        var buildHostProcessManager = new BuildHostProcessManager(
            globalMSBuildProperties: null,
            binaryLogPathProvider: null,
            loggerFactory: loggerFactoryMock.Object);

        var projectOrSolutionFilePath = "path/to/project.csproj";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, projectOrSolutionFilePath, null, cancellationToken);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetBuildHostAsync_ReturnsBuildHost_WhenDotnetPathIsSame()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

        var buildHostProcessMock = new Mock<BuildHostProcess>(null, null, null);
        buildHostProcessMock.Setup(b => b.BuildHost.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MSBuildLocation { Path = "path/to/msbuild" });

        var buildHostProcessManager = new BuildHostProcessManager(
            globalMSBuildProperties: null,
            binaryLogPathProvider: null,
            loggerFactory: loggerFactoryMock.Object);

        var projectOrSolutionFilePath = "path/to/project.csproj";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, projectOrSolutionFilePath, "path/to/dotnet", cancellationToken);

        // Assert
        Assert.NotNull(result);
    }
}
