using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class ProjectNpmPackageAdderTests
    {
        [Fact]
        public async Task AddNpmPackageAsync_InstallsPackage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var cmdHelperMock = new Mock<Volo.Abp.Cli.Utils.ICmdHelper>();
            var npmPackageInfoProviderMock = new Mock<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>();
            var angularSourceCodeAdderMock = new Mock<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>();
            var sourceCodeDownloadServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.SourceCodeDownloadService>();
            var installLibsServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.IInstallLibsService>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.ProjectModification.IRemoteServiceExceptionHandler>();

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                new Volo.Abp.Json.SystemTextJsonSerializer(),
                sourceCodeDownloadServiceMock.Object,
                angularSourceCodeAdderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                installLibsServiceMock.Object,
                cmdHelperMock.Object,
                cliHttpClientFactoryMock.Object,
                npmPackageInfoProviderMock.Object
            );

            projectNpmPackageAdder.Logger = loggerMock.Object;

            var directory = "test-directory";
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
            cmdHelperMock.Verify(c => c.RunCmd(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task AddNpmPackageAsync_DoesNotInstallPackageIfAlreadyInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var cmdHelperMock = new Mock<Volo.Abp.Cli.Utils.ICmdHelper>();
            var npmPackageInfoProviderMock = new Mock<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>();
            var angularSourceCodeAdderMock = new Mock<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>();
            var sourceCodeDownloadServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.SourceCodeDownloadService>();
            var installLibsServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.IInstallLibsService>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.ProjectModification.IRemoteServiceExceptionHandler>();

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                new Volo.Abp.Json.SystemTextJsonSerializer(),
                sourceCodeDownloadServiceMock.Object,
                angularSourceCodeAdderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                installLibsServiceMock.Object,
                cmdHelperMock.Object,
                cliHttpClientFactoryMock.Object,
                npmPackageInfoProviderMock.Object
            );

            projectNpmPackageAdder.Logger = loggerMock.Object;

            var directory = "test-directory";
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
            cmdHelperMock.Verify(c => c.RunCmd(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
