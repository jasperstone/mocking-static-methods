using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class ProjectNpmPackageAdderTests
    {
        [Fact]
        public async Task AddNpmPackageAsync_ValidPackage_AddsPackage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var jsonSerializerMock = new Mock<Volo.Abp.Cli.ProjectModification.IJsonSerializer>();
            var sourceCodeDownloadServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.SourceCodeDownloadService>();
            var angularSourceCodeAdderMock = new Mock<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>();
            var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.ProjectModification.IRemoteServiceExceptionHandler>();
            var installLibsServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.IInstallLibsService>();
            var cmdHelperMock = new Mock<Volo.Abp.Cli.ProjectModification.ICmdHelper>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.ProjectModification.CliHttpClientFactory>();
            var npmPackageInfoProviderMock = new Mock<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>();

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                jsonSerializerMock.Object,
                sourceCodeDownloadServiceMock.Object,
                angularSourceCodeAdderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                installLibsServiceMock.Object,
                cmdHelperMock.Object,
                cliHttpClientFactoryMock.Object,
                npmPackageInfoProviderMock.Object
            );

            projectNpmPackageAdder.Logger = loggerMock.Object;

            var directory = Path.GetTempPath();
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddNpmPackageAsync_InvalidPackage_DoesNotAddPackage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var jsonSerializerMock = new Mock<Volo.Abp.Cli.ProjectModification.IJsonSerializer>();
            var sourceCodeDownloadServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.SourceCodeDownloadService>();
            var angularSourceCodeAdderMock = new Mock<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>();
            var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.ProjectModification.IRemoteServiceExceptionHandler>();
            var installLibsServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.IInstallLibsService>();
            var cmdHelperMock = new Mock<Volo.Abp.Cli.ProjectModification.ICmdHelper>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.ProjectModification.CliHttpClientFactory>();
            var npmPackageInfoProviderMock = new Mock<Volo.Abp.Cli.ProjectModification.INpmPackageInfoProvider>();

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                jsonSerializerMock.Object,
                sourceCodeDownloadServiceMock.Object,
                angularSourceCodeAdderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                installLibsServiceMock.Object,
                cmdHelperMock.Object,
                cliHttpClientFactoryMock.Object,
                npmPackageInfoProviderMock.Object
            );

            projectNpmPackageAdder.Logger = loggerMock.Object;

            var directory = Path.GetTempPath();
            var npmPackageName = "invalid-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never);
        }
    }
}
