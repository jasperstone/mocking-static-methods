using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectNpmPackageAdderTests
    {
        private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<SourceCodeDownloadService> _sourceCodeDownloadServiceMock;
        private readonly Mock<AngularSourceCodeAdder> _angularSourceCodeAdderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<IInstallLibsService> _installLibsServiceMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<INpmPackageInfoProvider> _npmPackageInfoProviderMock;

        public ProjectNpmPackageAdderTests()
        {
            _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _sourceCodeDownloadServiceMock = new Mock<SourceCodeDownloadService>();
            _angularSourceCodeAdderMock = new Mock<AngularSourceCodeAdder>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _installLibsServiceMock = new Mock<IInstallLibsService>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation()
        {
            // Arrange
            var packageJsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "package.json");
            File.Create(packageJsonFilePath).Dispose();

            var npmPackage = new NpmPackageInfo { Name = "test-package" };

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                _jsonSerializerMock.Object,
                _sourceCodeDownloadServiceMock.Object,
                _angularSourceCodeAdderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _installLibsServiceMock.Object,
                _cmdHelperMock.Object,
                _cliHttpClientFactoryMock.Object,
                _npmPackageInfoProviderMock.Object
            );

            projectNpmPackageAdder.Logger = _loggerMock.Object;

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(Directory.GetCurrentDirectory(), npmPackage);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageIsAlreadyInstalled()
        {
            // Arrange
            var packageJsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "package.json");
            File.Create(packageJsonFilePath).Dispose();

            var npmPackage = new NpmPackageInfo { Name = "test-package" };

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                _jsonSerializerMock.Object,
                _sourceCodeDownloadServiceMock.Object,
                _angularSourceCodeAdderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _installLibsServiceMock.Object,
                _cmdHelperMock.Object,
                _cliHttpClientFactoryMock.Object,
                _npmPackageInfoProviderMock.Object
            );

            projectNpmPackageAdder.Logger = _loggerMock.Object;

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(Directory.GetCurrentDirectory(), npmPackage);
            await projectNpmPackageAdder.AddNpmPackageAsync(Directory.GetCurrentDirectory(), npmPackage);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
