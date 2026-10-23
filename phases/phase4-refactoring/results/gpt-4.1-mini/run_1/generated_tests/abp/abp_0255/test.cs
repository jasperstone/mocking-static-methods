using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands.Services
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadNugetPackageAsync_LogsInformationAfterDownload()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SourceCodeDownloadService>>();
            var mockNugetPackageProjectBuilder = new Mock<INugetPackageProjectBuilder>();
            var mockModuleProjectBuilder = new Mock<IModuleProjectBuilder>();
            var mockNpmPackageProjectBuilder = new Mock<INpmPackageProjectBuilder>();

            var service = new SourceCodeDownloadService(
                (ModuleProjectBuilder)mockModuleProjectBuilder.Object,
                (NugetPackageProjectBuilder)mockNugetPackageProjectBuilder.Object,
                (NpmPackageProjectBuilder)mockNpmPackageProjectBuilder.Object
            )
            {
                Logger = mockLogger.Object
            };

            var packageName = "TestPackage";
            var outputFolder = "TestOutputFolder";
            var version = "1.0.0";

            var buildResult = new ProjectBuildResult(new byte[0], packageName);

            mockNugetPackageProjectBuilder
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(buildResult);

            // Act
            await service.DownloadNugetPackageAsync(packageName, outputFolder, version);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{packageName}' has been successfully downloaded to '{outputFolder}'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }

    // Interfaces to allow mocking of the project builders
    public interface INugetPackageProjectBuilder
    {
        Task<ProjectBuildResult> BuildAsync(ProjectBuildArgs args);
    }

    public interface IModuleProjectBuilder
    {
        Task<ProjectBuildResult> BuildAsync(ProjectBuildArgs args);
    }

    public interface INpmPackageProjectBuilder
    {
        Task<ProjectBuildResult> BuildAsync(ProjectBuildArgs args);
    }
}
