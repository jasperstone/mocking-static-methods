using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;

public class ProjectNpmPackageAdderTests
{
    [Fact]
    public async Task AddNpmPackageAsync_LogsInformationMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();
        var npmPackageInfo = new NpmPackageInfo { Name = "test-package" };
        npmPackageInfoProviderMock.Setup(p => p.GetPackageListAsync()).ReturnsAsync(new[] { npmPackageInfo });

        var projectNpmPackageAdder = new ProjectNpmPackageAdder(
            null, // IJsonSerializer
            null, // SourceCodeDownloadService
            null, // AngularSourceCodeAdder
            null, // IRemoteServiceExceptionHandler
            null, // IInstallLibsService
            cmdHelperMock.Object,
            null, // CliHttpClientFactory
            npmPackageInfoProviderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        var directory = "test-directory";
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "package.json"), "{}");

        // Act
        await projectNpmPackageAdder.AddNpmPackageAsync(directory, "test-package");

        // Assert
        loggerMock.Verify(
            l => l.LogInformation(It.Is<string>(s => s.Contains("yarn add test-package"))),
            Times.Once
        );
    }
}
