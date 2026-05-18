using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;

public class SourceCodeDownloadServiceTests
{
    [Fact]
    public async Task DownloadModuleAsync_ShouldLogSuccessMessage()
    {
        // Arrange
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        var moduleName = "TestModule";
        var outputFolder = "TestOutputFolder";
        var version = "1.0.0";
        var gitHubAbpLocalRepositoryPath = "TestAbpRepoPath";
        var gitHubVoloLocalRepositoryPath = "TestVoloRepoPath";
        var options = new AbpCommandLineOptions();

        var projectBuildResult = new ProjectBuildResult(new byte[0], moduleName);
        moduleProjectBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(projectBuildResult);

        // Act
        await service.DownloadModuleAsync(moduleName, outputFolder, version, gitHubAbpLocalRepositoryPath, gitHubVoloLocalRepositoryPath, options);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s == $"'{moduleName}' has been successfully downloaded to '{outputFolder}'")),
            Times.Once
        );
    }
}
