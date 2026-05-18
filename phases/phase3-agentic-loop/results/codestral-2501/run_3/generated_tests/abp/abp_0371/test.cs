using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Json;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectModification
{
    public class ProjectNpmPackageAdderTests
    {
        private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<INpmPackageInfoProvider> _npmPackageInfoProviderMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<SourceCodeDownloadService> _sourceCodeDownloadServiceMock;
        private readonly Mock<AngularSourceCodeAdder> _angularSourceCodeAdderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<IInstallLibsService> _installLibsServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;

        private readonly ProjectNpmPackageAdder _projectNpmPackageAdder;

        public ProjectNpmPackageAdderTests()
        {
            _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _sourceCodeDownloadServiceMock = new Mock<SourceCodeDownloadService>();
            _angularSourceCodeAdderMock = new Mock<AngularSourceCodeAdder>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _installLibsServiceMock = new Mock<IInstallLibsService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

            _projectNpmPackageAdder = new ProjectNpmPackageAdder(
                _jsonSerializerMock.Object,
                _sourceCodeDownloadServiceMock.Object,
                _angularSourceCodeAdderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _installLibsServiceMock.Object,
                _cmdHelperMock.Object,
                _cliHttpClientFactoryMock.Object,
                _npmPackageInfoProviderMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageJsonExists()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            var packageJsonContent = "{}";

            _npmPackageInfoProviderMock.Setup(x => x.FindNpmPackageInfoAsync(npmPackageName))
                .ReturnsAsync(npmPackage);

            File.WriteAllText(packageJsonFilePath, packageJsonContent);

            // Act
            await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package to the project '{packageJsonFilePath}'...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            File.Delete(packageJsonFilePath);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogError_WhenPackageJsonDoesNotExist()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");

            _npmPackageInfoProviderMock.Setup(x => x.FindNpmPackageInfoAsync(npmPackageName))
                .ReturnsAsync(npmPackage);

            // Act
            await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "package.json not found!"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsAlreadyInstalled()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            var packageJsonContent = $"{{\"dependencies\": {{\"{npmPackageName}\": \"1.0.0\"}}}}";

            _npmPackageInfoProviderMock.Setup(x => x.FindNpmPackageInfoAsync(npmPackageName))
                .ReturnsAsync(npmPackage);

            File.WriteAllText(packageJsonFilePath, packageJsonContent);

            // Act
            await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{npmPackageName}' is already installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            File.Delete(packageJsonFilePath);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenAddingPackage()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            var packageJsonContent = "{}";

            _npmPackageInfoProviderMock.Setup(x => x.FindNpmPackageInfoAsync(npmPackageName))
                .ReturnsAsync(npmPackage);

            File.WriteAllText(packageJsonFilePath, packageJsonContent);

            // Act
            await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add " + npmPackageName)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            File.Delete(packageJsonFilePath);
        }
    }
}
