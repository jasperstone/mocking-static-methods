using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadNugetPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var service = new SourceCodeDownloadService(
                new ModuleProjectBuilder(),
                new NugetPackageProjectBuilder(),
                new NpmPackageProjectBuilder()
            );
            service.Logger = loggerMock.Object;

            var packageName = "TestPackage";
            var outputFolder = Path.GetTempPath();
            var version = "1.0.0";

            // Act
            await service.DownloadNugetPackageAsync(packageName, outputFolder, version);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains(packageName) && s.Contains(outputFolder))),
                Times.Once
            );
        }

        [Fact]
        public async Task DownloadModuleAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var service = new SourceCodeDownloadService(
                new ModuleProjectBuilder(),
                new NugetPackageProjectBuilder(),
                new NpmPackageProjectBuilder()
            );
            service.Logger = loggerMock.Object;

            var moduleName = "TestModule";
            var outputFolder = Path.GetTempPath();
            var version = "1.0.0";

            // Act
            await service.DownloadModuleAsync(moduleName, outputFolder, version, null, null, null);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains(moduleName) && s.Contains(outputFolder))),
                Times.Once
            );
        }

        [Fact]
        public async Task DownloadNpmPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var service = new SourceCodeDownloadService(
                new ModuleProjectBuilder(),
                new NugetPackageProjectBuilder(),
                new NpmPackageProjectBuilder()
            );
            service.Logger = loggerMock.Object;

            var packageName = "TestPackage";
            var outputFolder = Path.GetTempPath();
            var version = "1.0.0";

            // Act
            await service.DownloadNpmPackageAsync(packageName, outputFolder, version);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains(packageName) && s.Contains(outputFolder))),
                Times.Once
            );
        }
    }
}
