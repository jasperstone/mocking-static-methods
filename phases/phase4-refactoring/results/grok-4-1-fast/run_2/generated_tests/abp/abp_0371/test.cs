using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests;

public class ProjectNpmPackageAdderTests
{
    private readonly Mock<ILogger<ProjectNpmPackageAdder>> _mockLogger;
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly ProjectNpmPackageAdder _adder;

    public ProjectNpmPackageAdderTests()
    {
        _mockLogger = new Mock<ILogger<ProjectNpmPackageAdder>>();
        _mockLogger.SetupAllProperties();
        _mockCmdHelper = new Mock<ICmdHelper>();

        // Use NullLogger and minimal mocks for unavailable types
        var nullLogger = NullLogger<ProjectNpmPackageAdder>.Instance;
        var jsonSerializer = Mock.Of<IJsonSerializer>();
        var sourceCodeDownloadService = Mock.Of<SourceCodeDownloadService>();
        var angularSourceCodeAdder = Mock.Of<AngularSourceCodeAdder>();
        var remoteServiceExceptionHandler = Mock.Of<IRemoteServiceExceptionHandler>();
        var installLibsService = Mock.Of<IInstallLibsService>();
        var cliHttpClientFactory = Mock.Of<CliHttpClientFactory>();
        var npmPackageInfoProvider = Mock.Of<INpmPackageInfoProvider>();

        _mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>())).Returns(0);

        _adder = new ProjectNpmPackageAdder(
            jsonSerializer,
            sourceCodeDownloadService,
            angularSourceCodeAdder,
            remoteServiceExceptionHandler,
            installLibsService,
            _mockCmdHelper.Object,
            cliHttpClientFactory,
            npmPackageInfoProvider
        )
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task AddNpmPackageAsync_Should_Log_YarnAddCommand_When_Package_Not_Present()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var npmPackage = new NpmPackageInfo { Name = "@abp/theme-shared" };
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(packageJsonPath, "{}");

        try
        {
            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage);

            // Assert - verifies line 83 LogInformation call
            _mockLogger.Verify(
                x => x.LogInformation("yarn add @abp/theme-shared"),
                Times.Once);
        }
        finally
        {
            try
            {
                if (File.Exists(packageJsonPath))
                    File.Delete(packageJsonPath);
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task AddNpmPackageAsync_Should_Log_YarnAddCommand_WithVersion_When_Package_Not_Present()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var npmPackage = new NpmPackageInfo { Name = "@abp/theme-shared" };
        var version = "1.2.3";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(packageJsonPath, "{}");

        try
        {
            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage, version);

            // Assert - verifies line 83 LogInformation with version postfix
            _mockLogger.Verify(
                x => x.LogInformation("yarn add @abp/theme-shared@1.2.3"),
                Times.Once);
        }
        finally
        {
            try
            {
                if (File.Exists(packageJsonPath))
                    File.Delete(packageJsonPath);
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task AddNpmPackageAsync_Should_Log_AlreadyInstalled_When_Package_Present()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var npmPackage = new NpmPackageInfo { Name = "@abp/theme-shared" };
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(packageJsonPath, "{\"dependencies\":{\"@abp/theme-shared\":\"1.0.0\"}}");

        try
        {
            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(m => m.Contains("'@abp/theme-shared' is already installed."))),
                Times.Once);
        }
        finally
        {
            try
            {
                if (File.Exists(packageJsonPath))
                    File.Delete(packageJsonPath);
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
