using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;

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
            new DefaultJsonSerializer(),
            new SourceCodeDownloadService(new DefaultJsonSerializer(), new CliHttpClientFactory(), new NullLogger<SourceCodeDownloadService>()),
            new AngularSourceCodeAdder(new DefaultJsonSerializer(), new CliHttpClientFactory(), new NullLogger<AngularSourceCodeAdder>()),
            new NullLogger<IRemoteServiceExceptionHandler>(),
            new InstallLibsService(new DefaultJsonSerializer(), new CliHttpClientFactory(), new NullLogger<InstallLibsService>()),
            cmdHelperMock.Object,
            new CliHttpClientFactory(),
            npmPackageInfoProviderMock.Object)
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
            l => l.LogInformation("yarn add test-package"),
            Times.Once);
    }
}
