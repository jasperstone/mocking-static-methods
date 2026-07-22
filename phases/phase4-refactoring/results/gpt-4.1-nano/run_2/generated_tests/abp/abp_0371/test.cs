using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectModification;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectNpmPackageAdderTests
    {
        [Fact]
        public async Task AddNpmPackageAsync_Should_LogYarnAddCommand()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var sourceCodeDownloadServiceMock = new Mock<SourceCodeDownloadService>();
            var angularSourceCodeAdderMock = new Mock<AngularSourceCodeAdder>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var installLibsServiceMock = new Mock<IInstallLibsService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();

            var adder = new ProjectNpmPackageAdder(
                jsonSerializerMock.Object,
                sourceCodeDownloadServiceMock.Object,
                angularSourceCodeAdderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                installLibsServiceMock.Object,
                cmdHelperMock.Object,
                cliHttpClientFactoryMock.Object,
                npmPackageInfoProviderMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            var testDirectory = Path.Combine(Path.GetTempPath(), "TestProject");
            Directory.CreateDirectory(testDirectory);
            var packageJsonPath = Path.Combine(testDirectory, "package.json");
            var packageName = "test-package";

            // Create a dummy package.json with the package name
            File.WriteAllText(packageJsonPath, $"{{\"name\": \"{packageName}\"}}");

            // Setup the NpmPackageInfo
            var npmInfo = new NpmPackageInfo { Name = packageName };

            // Act
            await adder.AddNpmPackageAsync(testDirectory, npmInfo);

            // Assert
            // Verify that LogInformation was called with the yarn add command
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"yarn add {packageName}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
