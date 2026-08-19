using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Commands.Services.Tests
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadModuleAsync_ShouldLogSuccessMessage()
        {
            // Arrange
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(MockBehavior.Strict);
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>(MockBehavior.Strict);
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object)
            {
                Logger = loggerMock.Object
            };

            var moduleName = "TestModule";
            var outputFolder = "TestOutputFolder";
            var version = "1.0.0";
            var gitHubAbpLocalRepositoryPath = "TestAbpLocalRepositoryPath";
            var gitHubVoloLocalRepositoryPath = "TestVoloLocalRepositoryPath";
            var options = new AbpCommandLineOptions();

            var buildResult = new ProjectBuildResult(new byte[0], outputFolder);

            moduleProjectBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(buildResult);

            // Act
            await service.DownloadModuleAsync(moduleName, outputFolder, version, gitHubAbpLocalRepositoryPath, gitHubVoloLocalRepositoryPath, options);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s == $"'{moduleName}' has been successfully downloaded to '{outputFolder}'")),
                Times.Once);
        }
    }
}
