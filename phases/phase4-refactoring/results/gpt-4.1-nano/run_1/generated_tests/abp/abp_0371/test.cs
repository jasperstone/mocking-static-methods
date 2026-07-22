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
        public async Task AddNpmPackageAsync_Should_LogInformation_When_PackageNotInstalled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var mockSourceCodeDownloadService = new Mock<SourceCodeDownloadService>();
            var mockAngularSourceCodeAdder = new Mock<AngularSourceCodeAdder>();
            var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockInstallLibsService = new Mock<IInstallLibsService>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockNpmPackageInfoProvider = new Mock<INpmPackageInfoProvider>();

            var adder = new ProjectNpmPackageAdder(
                null,
                mockSourceCodeDownloadService.Object,
                mockAngularSourceCodeAdder.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockInstallLibsService.Object,
                mockCmdHelper.Object,
                mockHttpClientFactory.Object,
                mockNpmPackageInfoProvider.Object
            )
            {
                Logger = mockLogger.Object
            };

            var directory = "testDir";
            var npmPackageName = "testPackage";

            var npmPackageInfo = new NpmPackageInfo { Name = npmPackageName };

            // Mock static methods
            // For simplicity, assume File.Exists returns true for package.json
            // and File.ReadAllText returns a string that does not contain the package name
            // You might need to abstract static calls for better testability

            // Act
            await adder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation($"Installing '{npmPackageName}' package to the project '{Path.Combine(directory, "package.json")}'..."),
                Times.Once);
        }
    }
}
