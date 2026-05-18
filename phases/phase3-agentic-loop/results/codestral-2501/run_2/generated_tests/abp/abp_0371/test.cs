using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.IO;
using Xunit;

public class ProjectNpmPackageAdderTests
{
    private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly ProjectNpmPackageAdder _projectNpmPackageAdder;

    public ProjectNpmPackageAdderTests()
    {
        _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
        _cmdHelperMock = new Mock<ICmdHelper>();

        _projectNpmPackageAdder = new ProjectNpmPackageAdder(
            null,
            null,
            null,
            null,
            null,
            _cmdHelperMock.Object,
            null,
            null)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogError_WhenPackageJsonNotFound()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

        // Assert
        _loggerMock.Verify(
            x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageJsonFound()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var packageJsonContent = "{}";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        File.WriteAllText(packageJsonPath, packageJsonContent);

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));

        // Clean up
        Directory.Delete(directory, true);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageAlreadyInstalled()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var packageJsonContent = $"{{\"dependencies\": {{\"{npmPackageName}\": \"1.0.0\"}}}}";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        File.WriteAllText(packageJsonPath, packageJsonContent);

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));

        // Clean up
        Directory.Delete(directory, true);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldRunCmd_WhenPackageNotInstalled()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var packageJsonContent = "{}";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        File.WriteAllText(packageJsonPath, packageJsonContent);

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

        // Assert
        _cmdHelperMock.Verify(
            x => x.RunCmd(It.IsAny<string>()),
            Times.Once);

        // Clean up
        Directory.Delete(directory, true);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsInstalled()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var packageJsonContent = $"{{\"dependencies\": {{\"{npmPackageName}\": \"1.0.0\"}}}}";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        File.WriteAllText(packageJsonPath, packageJsonContent);

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));

        // Clean up
        Directory.Delete(directory, true);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsInstalledWithVersion()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var version = "1.0.0";
        var packageJsonContent = $"{{\"dependencies\": {{\"{npmPackageName}\": \"{version}\"}}}}";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        File.WriteAllText(packageJsonPath, packageJsonContent);

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));

        // Clean up
        Directory.Delete(directory, true);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsInstalledWithVersionAndSourceCode()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var version = "1.0.0";
        var packageJsonContent = $"{{\"dependencies\": {{\"{npmPackageName}\": \"{version}\"}}}}";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        File.WriteAllText(packageJsonPath, packageJsonContent);

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version, true);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(3));

        // Clean up
        Directory.Delete(directory, true);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsInstalledWithSourceCode()
    {
        // Arrange
        var directory = "test-directory";
        var npmPackageName = "test-package";
        var packageJsonContent = $"{{\"dependencies\": {{\"{npmPackageName}\": \"1.0.0\"}}}}";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        File.WriteAllText(packageJsonPath, packageJsonContent);

        // Act
        await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, withSourceCode: true);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(3));

        // Clean up
        Directory.Delete(directory, true);
    }
}
