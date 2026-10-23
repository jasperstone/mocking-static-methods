using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectModification
{
    public class ProjectNpmPackageAdderTests
    {
        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonExistsAndPackageNotInstalled()
        {
            // Arrange
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            var npmPackageName = "test-package";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var version = "1.0.0";

            // Write a package.json file that does not contain the package name
            await File.WriteAllTextAsync(packageJsonPath, "{ \"dependencies\": {} }");

            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();
            var angularSourceCodeAdderMock = new Mock<AngularSourceCodeAdder>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var installLibsServiceMock = new Mock<IInstallLibsService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();

            // We cannot mock SourceCodeDownloadService due to no parameterless constructor, so use null or a dummy subclass
            var sourceCodeDownloadService = new DummySourceCodeDownloadService();

            var adder = new ProjectNpmPackageAdder(
                jsonSerializerMock.Object,
                sourceCodeDownloadService,
                angularSourceCodeAdderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                installLibsServiceMock.Object,
                cmdHelperMock.Object,
                cliHttpClientFactoryMock.Object,
                npmPackageInfoProviderMock.Object
            );
            adder.Logger = loggerMock.Object;

            // Act
            await adder.AddNpmPackageAsync(directory, npmPackage, version);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add " + npmPackageName + "@" + version)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            cmdHelperMock.Verify(x => x.RunCmd(It.Is<string>(s => s.Contains("npx yarn add " + npmPackageName + "@" + version))), Times.Once);

            // Cleanup
            Directory.Delete(directory, true);
        }

        private class DummySourceCodeDownloadService : SourceCodeDownloadService
        {
            // No override needed for this test
        }
    }
}
