using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectModification;

public class ProjectNpmPackageAdderTests
{
    private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly Mock<INpmPackageInfoProvider> _npmPackageInfoProviderMock;
    private readonly ProjectNpmPackageAdder _projectNpmPackageAdder;

    public ProjectNpmPackageAdderTests()
    {
        _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
        _cmdHelperMock = new Mock<ICmdHelper>();
        _npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();

        _projectNpmPackageAdder = new ProjectNpmPackageAdder(
            null, // IJsonSerializer
            null, // SourceCodeDownloadService
            null, // AngularSourceCodeAdder
            null, // IRemoteServiceExceptionHandler
            null, // IInstallLibsService
            _cmdHelperMock.Object,
            null, // CliHttpClientFactory
            _npmPackageInfoProviderMock.Object
        )
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task AddNpmPackageAsync_LogsInformationMessage_WhenPackageIsNotInstalled()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackage = new NpmPackageInfo { Name = "test-package" };
        var version = "1.0.0";
        var packageJsonContent = $"{{\"dependencies\": {{}}}}";
        File.WriteAllText(Path.Combine(directory, "package.json"), packageJsonContent);

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackage, version);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("yarn add test-package@1.0.0")),
                It.IsAny<Exception>(),
                It.IsAny<ILoggerLogOptions>(),
                It.IsAny<State>(),
                It.IsAny<Func<string, Exception, string>>()),
            Times.Once);
    }
}
