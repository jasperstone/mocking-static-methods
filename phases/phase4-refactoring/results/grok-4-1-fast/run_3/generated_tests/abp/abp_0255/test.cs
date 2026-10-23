using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests;

public class SourceCodeDownloadServiceTests
{
    [Fact]
    public async Task DownloadModuleAsync_Should_Log_Successful_Download()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

        var minimalZip = new byte[] { 0x50, 0x4B, 0x05, 0x06 };
        moduleProjectBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(minimalZip, ""));

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await service.DownloadModuleAsync(
            "TestModule",
            "/tmp/output",
            "1.0.0",
            null,
            null,
            new AbpCommandLineOptions()
        );

        // Assert
        moduleProjectBuilderMock.Verify(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()), Times.Once);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(4) // 3 initial logs + 1 success log
        );
    }

    [Fact]
    public async Task DownloadNugetPackageAsync_Should_Log_Successful_Download()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

        var minimalZip = new byte[] { 0x50, 0x4B, 0x05, 0x06 };
        nugetPackageProjectBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(minimalZip, ""));

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await service.DownloadNugetPackageAsync(
            "Test.Package",
            "/tmp/output",
            "1.0.0"
        );

        // Assert
        nugetPackageProjectBuilderMock.Verify(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()), Times.Once);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(4) // 3 initial logs + 1 success log
        );
    }

    [Fact]
    public async Task DownloadNpmPackageAsync_Should_Log_Successful_Download()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

        var minimalZip = new byte[] { 0x50, 0x4B, 0x05, 0x06 };
        npmPackageProjectBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(minimalZip, ""));

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await service.DownloadNpmPackageAsync(
            "@abp/test",
            "/tmp/output",
            "1.0.0"
        );

        // Assert
        npmPackageProjectBuilderMock.Verify(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()), Times.Once);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(4) // 3 initial logs + 1 success log
        );
    }
}
