using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services;

public class SourceCodeDownloadServiceTests
{
    [Fact]
    public async Task DownloadModuleAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(MockBehavior.Strict, 
            Mock.Of<ISourceCodeStore>(), 
            Mock.Of<IModuleInfoProvider>(), 
            new object(), 
            new object(), 
            new object(), 
            new object());
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>(MockBehavior.Strict);
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>(MockBehavior.Strict);
        var sourceCodeDownloadService = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        );
        sourceCodeDownloadService.Logger = loggerMock.Object;

        // Act
        await sourceCodeDownloadService.DownloadModuleAsync(
            "moduleName",
            "outputFolder",
            "version",
            "gitHubAbpLocalRepositoryPath",
            "gitHubVoloLocalRepositoryPath",
            new AbpCommandLineOptions()
        );

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(It.Is<string>(s => s.Contains("moduleName") && s.Contains("outputFolder"))),
            Times.Once
        );
    }

    [Fact]
    public async Task DownloadNugetPackageAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(MockBehavior.Strict, 
            Mock.Of<ISourceCodeStore>(), 
            Mock.Of<IModuleInfoProvider>(), 
            new object(), 
            new object(), 
            new object(), 
            new object());
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>(MockBehavior.Strict);
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>(MockBehavior.Strict);
        var sourceCodeDownloadService = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        );
        sourceCodeDownloadService.Logger = loggerMock.Object;

        // Act
        await sourceCodeDownloadService.DownloadNugetPackageAsync(
            "packageName",
            "outputFolder",
            "version"
        );

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(It.Is<string>(s => s.Contains("packageName") && s.Contains("outputFolder"))),
            Times.Once
        );
    }

    [Fact]
    public async Task DownloadNpmPackageAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(MockBehavior.Strict, 
            Mock.Of<ISourceCodeStore>(), 
            Mock.Of<IModuleInfoProvider>(), 
            new object(), 
            new object(), 
            new object(), 
            new object());
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>(MockBehavior.Strict);
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>(MockBehavior.Strict);
        var sourceCodeDownloadService = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        );
        sourceCodeDownloadService.Logger = loggerMock.Object;

        // Act
        await sourceCodeDownloadService.DownloadNpmPackageAsync(
            "packageName",
            "outputFolder",
            "version"
        );

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(It.Is<string>(s => s.Contains("packageName") && s.Contains("outputFolder"))),
            Times.Once
        );
    }
}
