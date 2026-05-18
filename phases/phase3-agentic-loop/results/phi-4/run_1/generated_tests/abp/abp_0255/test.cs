using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands.Services;

public class SourceCodeDownloadServiceTests
{
    [Fact]
    public async Task DownloadModuleAsync_LogsSuccessMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
        var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
        var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

        var service = new SourceCodeDownloadService(
            moduleProjectBuilderMock.Object,
            nugetPackageProjectBuilderMock.Object,
            npmPackageProjectBuilderMock.Object)
        {
            Logger = loggerMock.Object
        };

        var projectBuildArgs = new ProjectBuildArgs(
            SolutionName.Parse("TestModule"),
            "TestModule",
            null,
            "outputFolder",
            DatabaseProvider.NotSpecified,
            DatabaseManagementSystem.NotSpecified,
            UiFramework.NotSpecified,
            null,
            false,
            null,
            null,
            null,
            new AbpCommandLineOptions());

        var result = new ProjectBuildResult
        {
            ZipContent = new byte[0]
        };

        moduleProjectBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(result);

        // Act
        await service.DownloadModuleAsync("TestModule", "outputFolder", null, null, null, new AbpCommandLineOptions());

        // Assert
        loggerMock.Verify(
            l => l.LogInformation($"'{It.Is<string>(s => s.Contains("TestModule"))}' has been successfully downloaded to 'outputFolder'"),
            Times.Once);
    }
}
