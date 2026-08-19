using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
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

        moduleProjectBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, ""));

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        try
        {
            Directory.CreateDirectory(outputFolder);

            // Act
            await service.DownloadModuleAsync("TestModule", outputFolder, "1.0.0", null, null, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("'TestModule' has been successfully downloaded") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }
    }

    [Fact]
    public async Task DownloadNugetPackageAsync_Should_Log_Successful_Download()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

        nugetPackageProjectBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, ""));

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        try
        {
            Directory.CreateDirectory(outputFolder);

            // Act
            await service.DownloadNugetPackageAsync("Test.Package", outputFolder, "1.0.0");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("'Test.Package' has been successfully downloaded") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }
    }

    [Fact]
    public async Task DownloadNpmPackageAsync_Should_Log_Successful_Download()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

        npmPackageProjectBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, ""));

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        try
        {
            Directory.CreateDirectory(outputFolder);

            // Act
            await service.DownloadNpmPackageAsync("test-package", outputFolder, "1.0.0");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("'test-package' has been successfully downloaded") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }
    }
}
