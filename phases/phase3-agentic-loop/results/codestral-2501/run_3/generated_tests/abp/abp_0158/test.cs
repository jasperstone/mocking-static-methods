using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Memory;
using Volo.Abp.Cli.Utils;
using Xunit;

public class CliServiceTests
{
    [Fact]
    public async Task CheckCliVersionAsync_ShouldLogNewVersionInfo()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var memoryServiceMock = new Mock<MemoryService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        var cliService = new CliService(
            null, null, null, packageVersionCheckerServiceMock.Object, null, memoryServiceMock.Object, cliVersionServiceMock.Object, null)
        {
            Logger = loggerMock.Object
        };

        var currentCliVersion = new SemanticVersion(1, 0, 0);
        var latestVersionInfo = new LatestVersionInfo(new SemanticVersion(1, 1, 0), "New version available");

        memoryServiceMock.Setup(x => x.GetAsync(CliConsts.MemoryKeys.LatestCliVersionCheckDate)).ReturnsAsync("2023-01-01");
        packageVersionCheckerServiceMock.Setup(x => x.GetLatestVersionAsync(It.IsAny<CliService.UpdateChannel>())).ReturnsAsync(latestVersionInfo);

        // Act
        await cliService.CheckCliVersionAsync(currentCliVersion);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer stable version of the ABP CLI is available: 1.1.0.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update Command: ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool update --tool-path")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
