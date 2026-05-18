using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectModification;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Core.Tests
{
    public class ProjectNpmPackageAdderTests
    {
        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                Mock.Of<Volo.Abp.Json.IJsonSerializer>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.SourceCodeDownloadService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.IRemoteServiceExceptionHandler>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.IInstallLibsService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>()
            );
            projectNpmPackageAdder.Logger = loggerMock.Object;
            var directory = "test-directory";
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddNpmPackageAsync_DoesNotLogInformation_WhenPackageAlreadyInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                Mock.Of<Volo.Abp.Json.IJsonSerializer>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.SourceCodeDownloadService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.IRemoteServiceExceptionHandler>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.IInstallLibsService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>()
            );
            projectNpmPackageAdder.Logger = loggerMock.Object;
            var directory = "test-directory";
            var npmPackageName = "test-package";
            var version = "1.0.0";
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonFilePath, $"{{\"dependencies\":{{\"{npmPackageName}\":\"{version}\"}}}}");

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsError_WhenPackageJsonNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                Mock.Of<Volo.Abp.Json.IJsonSerializer>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.SourceCodeDownloadService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.IRemoteServiceExceptionHandler>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.IInstallLibsService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>()
            );
            projectNpmPackageAdder.Logger = loggerMock.Object;
            var directory = "test-directory";
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
