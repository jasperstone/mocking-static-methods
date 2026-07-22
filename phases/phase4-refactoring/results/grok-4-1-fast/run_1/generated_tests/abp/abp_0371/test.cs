using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
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
        _mockCmdHelper = new Mock<ICmdHelper>();
        
        // Create mocks only for types we can reference from the file
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var cmdHelperMock = _mockCmdHelper.Object;

        // Use object for concrete types we can't mock without additional references
        var sourceCodeDownloadService = new object();
        var angularSourceCodeAdder = new object();
        var remoteServiceExceptionHandler = new object();
        var installLibsService = new object();
        var cliHttpClientFactory = new object();
        var npmPackageInfoProvider = new object();

        _adder = new ProjectNpmPackageAdder(
            jsonSerializerMock.Object,
            (Volo.Abp.Cli.Commands.Services.SourceCodeDownloadService)sourceCodeDownloadService,
            (AngularSourceCodeAdder)angularSourceCodeAdder,
            (Volo.Abp.Cli.ProjectBuilding.IRemoteServiceExceptionHandler)remoteServiceExceptionHandler,
            (Volo.Abp.Cli.LIbs.IInstallLibsService)installLibsService,
            cmdHelperMock,
            (Volo.Abp.Cli.Http.CliHttpClientFactory)cliHttpClientFactory,
            (Volo.Abp.Cli.ProjectBuilding.INpmPackageInfoProvider)npmPackageInfoProvider
        )
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogYarnAddCommand_WhenPackageNotInstalled_WithVersion()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var directory = tempDir;
        var npmPackage = new NpmPackageInfo { Name = "@abp/theme-shared" };
        var version = "1.2.3";

        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            await File.WriteAllTextAsync(packageJsonPath, "{}"); // package.json exists but doesn't contain package

            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage, version);

            // Assert - Verifies the LogInformation call on line 83
            _mockLogger.Verify(
                x => x.LogInformation(
                    "yarn add @abp/theme-shared@1.2.3",
                    It.IsAny<object[]>()
                ),
                Times.Once()
            );
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogYarnAddCommand_WhenPackageNotInstalled_NoVersion()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var directory = tempDir;
        var npmPackage = new NpmPackageInfo { Name = "@abp/some-package" };

        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            await File.WriteAllTextAsync(packageJsonPath, "{}");

            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage);

            // Assert - logs "yarn add @abp/some-package" (no version postfix)
            _mockLogger.Verify(
                x => x.LogInformation(
                    "yarn add @abp/some-package",
                    It.IsAny<object[]>()
                ),
                Times.Once()
            );
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogAlreadyInstalled_WhenPackageAlreadyPresent()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var directory = tempDir;
        var npmPackage = new NpmPackageInfo { Name = "@abp/existing-package" };

        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            await File.WriteAllTextAsync(packageJsonPath, "{\"dependencies\": {\"@abp/existing-package\": \"^1.0.0\"}}");

            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    It.Is<string>(msg => msg.Contains("@abp/existing-package") && msg.Contains("already installed")),
                    It.IsAny<object[]>()
                ),
                Times.Once()
            );
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}
