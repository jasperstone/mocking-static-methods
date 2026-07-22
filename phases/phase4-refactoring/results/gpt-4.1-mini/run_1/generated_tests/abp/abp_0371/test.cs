using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectModification
{
    public class ProjectNpmPackageAdderTests
    {
        private class TestSourceCodeDownloadService : SourceCodeDownloadService
        {
            public override Task DownloadNpmPackageAsync(string packageName, string targetFolder, string version)
            {
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonExistsAndPackageNotInstalled()
        {
            // Arrange
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            var npmPackageName = "test-package";
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

            var sourceCodeDownloadService = new TestSourceCodeDownloadService();

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
            await adder.AddNpmPackageAsync(directory, npmPackageName, version);

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

            cmdHelperMock.Verify(x => x.RunCmd(It.Is<string>(s => s.Contains("yarn add " + npmPackageName + "@" + version))), Times.Once);

            // Cleanup
            Directory.Delete(directory, true);
        }
    }
}
