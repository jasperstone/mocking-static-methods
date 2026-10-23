using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.IO;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests;

public class ProjectNpmPackageAdderTests
{
    private readonly Mock<ILogger<ProjectNpmPackageAdder>> _mockLogger;
    private readonly ProjectNpmPackageAdder _adder;

    public ProjectNpmPackageAdderTests()
    {
        _mockLogger = new Mock<ILogger<ProjectNpmPackageAdder>>();

        // Create mocks only for types explicitly available from the source file usings
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        
        // Use NullLogger and Object for other dependencies to avoid type resolution issues
        var sourceCodeDownloadService = new object();
        var angularSourceCodeAdder = new object();
        var remoteServiceExceptionHandler = new object();
        var installLibsService = new object();
        var cliHttpClientFactory = new object();
        var npmPackageInfoProvider = new object();

        _adder = new ProjectNpmPackageAdder(
            jsonSerializerMock.Object,
            sourceCodeDownloadService,
            angularSourceCodeAdder,
            remoteServiceExceptionHandler,
            installLibsService,
            cmdHelperMock.Object,
            cliHttpClientFactory,
            npmPackageInfoProvider
        )
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogYarnAddCommand_WhenPackageNotInstalled()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            await File.WriteAllTextAsync(packageJsonPath, "{}");

            var npmPackage = new NpmPackageInfo { Name = "@abp/theme-shared" };

            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage);

            // Assert - Verify the LogInformation call on line 83
            _mockLogger.Verify(
                x => x.LogInformation("yarn add @abp/theme-shared"),
                Times.Once
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogYarnAddCommandWithVersion_WhenVersionSpecified()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            await File.WriteAllTextAsync(packageJsonPath, "{}");

            var npmPackage = new NpmPackageInfo { Name = "@abp/my-package" };
            var version = "1.0.0";

            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage, version);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation("yarn add @abp/my-package@1.0.0"),
                Times.Once
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogAlreadyInstalled_WhenPackageExists()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            await File.WriteAllTextAsync(packageJsonPath, "{\"dependencies\": {\"@abp/theme-shared\": \"1.0.0\"}}");

            var npmPackage = new NpmPackageInfo { Name = "@abp/theme-shared" };

            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage);

            // Assert - logs "already installed" message, not the yarn command
            _mockLogger.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("'@abp/theme-shared' is already installed."))), Times.Once);
            _mockLogger.Verify(x => x.LogInformation("yarn add @abp/theme-shared"), Times.Never);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public async Task AddMvcPackageAsync_ShouldLogYarnAddCommand()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            await File.WriteAllTextAsync(packageJsonPath, "{}");

            var npmPackage = new NpmPackageInfo { Name = "@abp/ng.theme.shared" };

            // Act
            await _adder.AddMvcPackageAsync(directory, npmPackage);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation("yarn add @abp/ng.theme.shared"),
                Times.Once
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
