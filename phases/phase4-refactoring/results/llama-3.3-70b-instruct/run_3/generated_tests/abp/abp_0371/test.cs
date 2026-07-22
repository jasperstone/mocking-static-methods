using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectNpmPackageAdderTests
    {
        [Fact]
        public async Task AddNpmPackageAsync_InstallsPackage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                loggerMock.Object,
                Mock.Of<Volo.Abp.Cli.Utils.IJsonSerializer>(),
                Mock.Of<Volo.Abp.Cli.ProjectBuilding.SourceCodeDownloadService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>(),
                Mock.Of<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>(),
                Mock.Of<Volo.Abp.Cli.ProjectBuilding.IInstallLibsService>(),
                Mock.Of<Volo.Abp.Cli.LIbs.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>()
            );

            var directory = Path.GetTempPath();
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddNpmPackageAsync_DoesNotInstallPackageIfAlreadyInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                loggerMock.Object,
                Mock.Of<Volo.Abp.Cli.Utils.IJsonSerializer>(),
                Mock.Of<Volo.Abp.Cli.ProjectBuilding.SourceCodeDownloadService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>(),
                Mock.Of<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>(),
                Mock.Of<Volo.Abp.Cli.ProjectBuilding.IInstallLibsService>(),
                Mock.Of<Volo.Abp.Cli.LIbs.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>()
            );

            var directory = Path.GetTempPath();
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Create a package.json file with the package already installed
            var packageJsonFile = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonFile, $"{{ \"dependencies\": {{ \"{npmPackageName}\": \"{version}\" }} }}");

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never);
        }
    }
}
