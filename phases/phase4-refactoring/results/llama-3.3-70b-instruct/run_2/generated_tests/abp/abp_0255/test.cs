using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadModuleAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(MockBehavior.Strict);
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            service.Logger = loggerMock.Object;

            // Act
            await service.DownloadModuleAsync("moduleName", "outputFolder", "version", "gitHubAbpLocalRepositoryPath", "gitHubVoloLocalRepositoryPath", new AbpCommandLineOptions());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DownloadNugetPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(MockBehavior.Strict);
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            service.Logger = loggerMock.Object;

            // Act
            await service.DownloadNugetPackageAsync("packageName", "outputFolder", "version");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DownloadNpmPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(MockBehavior.Strict);
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            service.Logger = loggerMock.Object;

            // Act
            await service.DownloadNpmPackageAsync("packageName", "outputFolder", "version");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
