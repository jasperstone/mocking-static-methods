using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BuildHostProcessManagerTests
{
    [Fact]
    public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenMonoMSBuildNotFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

        var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

        var projectFilePath = "path/to/project.csproj";
        var cancellationToken = CancellationToken.None;

        // Act
        await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.Mono, projectFilePath, cancellationToken);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"An installation of Mono MSBuild could not be found; {projectFilePath} will be loaded with the .NET Core SDK and may encounter errors.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenNetFrameworkMSBuildNotFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

        var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

        var projectFilePath = "path/to/project.csproj";
        var cancellationToken = CancellationToken.None;

        var remoteBuildHostMock = new Mock<RemoteBuildHost>(MockBehavior.Strict);
        remoteBuildHostMock.Setup(x => x.HasUsableMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        await buildHostProcessManager.GetBuildHostWithFallbackAsync(BuildHostProcessKind.NetFramework, projectFilePath, cancellationToken);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"An installation of Visual Studio or the Build Tools for Visual Studio could not be found; {projectFilePath} will be loaded with the .NET Core SDK and may encounter errors.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
