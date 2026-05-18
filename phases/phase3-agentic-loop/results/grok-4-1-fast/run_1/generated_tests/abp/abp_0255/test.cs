using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services;

public class SourceCodeDownloadServiceTests
{
    [Fact]
    public async Task DownloadModuleAsync_Should_Log_Successful_Download()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var fakeZipContent = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // Minimal ZIP header
        
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        moduleProjectBuilderMock
            .SetupGet(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(fakeZipContent, "test"));

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            Mock.Of<NugetPackageProjectBuilder>(),
            Mock.Of<NpmPackageProjectBuilder>()
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await service.DownloadModuleAsync("TestModule", "/tmp/output", "1.0.0", null, null, null);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    func.Invoke(null, null)!.Contains("'TestModule' has been successfully downloaded to '/tmp/output'"))),
            Times.Once
        );
    }

    [Fact]
    public async Task DownloadNugetPackageAsync_Should_Log_Successful_Download()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var fakeZipContent = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        nugetPackageProjectBuilderMock
            .SetupGet(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(fakeZipContent, "test"));

        var service = new SourceCodeDownloadService(
            Mock.Of<ModuleProjectBuilder>(),
            nugetPackageProjectBuilderMock.Object,
            Mock.Of<NpmPackageProjectBuilder>()
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await service.DownloadNugetPackageAsync("Test.Package", "/tmp/output", "1.0.0");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    func.Invoke(null, null)!.Contains("'Test.Package' has been successfully downloaded to '/tmp/output'"))),
            Times.Once
        );
    }

    [Fact]
    public async Task DownloadNpmPackageAsync_Should_Log_Successful_Download()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var fakeZipContent = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
        npmPackageProjectBuilderMock
            .SetupGet(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(fakeZipContent, "test"));

        var service = new SourceCodeDownloadService(
            Mock.Of<ModuleProjectBuilder>(),
            Mock.Of<NugetPackageProjectBuilder>(),
            npmPackageProjectBuilderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await service.DownloadNpmPackageAsync("test-package", "/tmp/output", "1.0.0");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    func.Invoke(null, null)!.Contains("'test-package' has been successfully downloaded to '/tmp/output'"))),
            Times.Once
        );
    }
}
