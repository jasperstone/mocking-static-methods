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
        public async Task AddNpmPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var npmPackageInfo = new NpmPackageInfo { Name = "test-package" };
            var jsonSerializerMock = new Mock<Volo.Abp.Json.IJsonSerializer>();
            var sourceCodeDownloadServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.SourceCodeDownloadService>();
            var angularSourceCodeAdderMock = new Mock<Volo.Abp.Cli.ProjectModification.AngularSourceCodeAdder>();
            var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.ProjectModification.IRemoteServiceExceptionHandler>();
            var installLibsServiceMock = new Mock<Volo.Abp.Cli.ProjectModification.IInstallLibsService>();
            var cmdHelperMock = new Mock<Volo.Abp.Cli.ProjectModification.ICmdHelper>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
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

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync("test-directory", npmPackageInfo);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
