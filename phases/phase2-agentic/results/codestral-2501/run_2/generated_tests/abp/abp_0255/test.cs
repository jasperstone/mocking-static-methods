using System;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;

public class SourceCodeDownloadServiceTests
{
    private readonly Mock<ILogger<SourceCodeDownloadService>> _loggerMock;
    private readonly Mock<ModuleProjectBuilder> _moduleProjectBuilderMock;
    private readonly Mock<NugetPackageProjectBuilder> _nugetPackageProjectBuilderMock;
    private readonly Mock<NpmPackageProjectBuilder> _npmPackageProjectBuilderMock;
    private readonly SourceCodeDownloadService _sourceCodeDownloadService;

    public SourceCodeDownloadServiceTests()
    {
        _loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        _moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        _nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        _npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

        _sourceCodeDownloadService = new SourceCodeDownloadService(
            _moduleProjectBuilderMock.Object,
            _nugetPackageProjectBuilderMock.Object,
            _npmPackageProjectBuilderMock.Object)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task DownloadModuleAsync_ShouldLogSuccessMessage()
    {
        // Arrange
        var moduleName = "TestModule";
        var outputFolder = "TestOutputFolder";
        var version = "1.0.0";
        var gitHubAbpLocalRepositoryPath = "TestAbpRepoPath";
        var gitHubVoloLocalRepositoryPath = "TestVoloRepoPath";
        var options = new AbpCommandLineOptions();

        var projectResult = new ProjectResult
        {
            ZipContent = new byte[0]
        };

        _moduleProjectBuilderMock
            .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(projectResult);

        // Act
        await _sourceCodeDownloadService.DownloadModuleAsync(moduleName, outputFolder, version, gitHubAbpLocalRepositoryPath, gitHubVoloLocalRepositoryPath, options);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation($"'{moduleName}' has been successfully downloaded to '{outputFolder}'"),
            Times.Once);
    }

    [Fact]
    public async Task DownloadNugetPackageAsync_ShouldLogSuccessMessage()
    {
        // Arrange
        var packageName = "TestPackage";
        var outputFolder = "TestOutputFolder";
        var version = "1.0.0";

        var projectResult = new ProjectResult
        {
            ZipContent = new byte[0]
        };

        _nugetPackageProjectBuilderMock
            .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(projectResult);

        // Act
        await _sourceCodeDownloadService.DownloadNugetPackageAsync(packageName, outputFolder, version);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation($"'{packageName}' has been successfully downloaded to '{outputFolder}'"),
            Times.Once);
    }

    [Fact]
    public async Task DownloadNpmPackageAsync_ShouldLogSuccessMessage()
    {
        // Arrange
        var packageName = "TestPackage";
        var outputFolder = "TestOutputFolder";
        var version = "1.0.0";

        var projectResult = new ProjectResult
        {
            ZipContent = new byte[0]
        };

        _npmPackageProjectBuilderMock
            .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(projectResult);

        // Act
        await _sourceCodeDownloadService.DownloadNpmPackageAsync(packageName, outputFolder, version);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation($"'{packageName}' has been successfully downloaded to '{outputFolder}'"),
            Times.Once);
    }
}
