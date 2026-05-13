using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.ProjectModification;

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
        private readonly ProjectNpmPackageAdder _projectNpmPackageAdder;

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
        public async Task AddNpmPackageAsync_Should_LogInformation_Call_LogInformation()
        {
            // Arrange
            var directory = "testDir";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonPath = Path.Combine(directory, "package.json");
            Directory.CreateDirectory(directory);
            File.WriteAllText(packageJsonPath, "{}");
            _npmPackageInfoProviderMock.Setup(p => p.GetPackageListAsync()).ReturnsAsync(new System.Collections.Generic.List<NpmPackageInfo> { npmPackage });
            _cmdHelperMock.Setup(c => c.RunCmd(It.IsAny<string>()));

            // Act
            await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackage, null, false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackage.Name}' package to the project")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
