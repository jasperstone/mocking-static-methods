using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Json;
using Xunit;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.IO;

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
            Mock.Of<IJsonSerializer>(),
            Mock.Of<SourceCodeDownloadService>(),
            Mock.Of<AngularSourceCodeAdder>(),
            Mock.Of<IRemoteServiceExceptionHandler>(),
            Mock.Of<IInstallLibsService>(),
            _cmdHelperMock.Object,
            Mock.Of<CliHttpClientFactory>(),
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
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var npmPackage = new NpmPackageInfo { Name = npmPackageName };
        var packageJsonFilePath = Path.Combine(directory, "package.json");

        // Mock file existence and content
        File.WriteAllText(packageJsonFilePath, "{}");

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package to the project '{packageJsonFilePath}'...")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Clean up
        File.Delete(packageJsonFilePath);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsAlreadyInstalled()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var npmPackage = new NpmPackageInfo { Name = npmPackageName };
        var packageJsonFilePath = Path.Combine(directory, "package.json");

        // Mock file existence and content
        File.WriteAllText(packageJsonFilePath, $"{{\"dependencies\": {{\"{npmPackageName}\": \"1.0.0\"}}}}");

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{npmPackageName}' is already installed.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Clean up
        File.Delete(packageJsonFilePath);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogInformation_WhenInstallingPackage()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var npmPackage = new NpmPackageInfo { Name = npmPackageName };
        var packageJsonFilePath = Path.Combine(directory, "package.json");

        // Mock file existence and content
        File.WriteAllText(packageJsonFilePath, "{}");

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add " + npmPackageName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Clean up
        File.Delete(packageJsonFilePath);
    }
}
