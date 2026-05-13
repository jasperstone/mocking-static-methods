using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests;

public class ProjectNpmPackageAdderTests
{
    private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly ProjectNpmPackageAdder _adder;

    public ProjectNpmPackageAdderTests()
    {
        _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
        _cmdHelperMock = new Mock<ICmdHelper>();

        // Create minimal mocks for required dependencies
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var sourceCodeDownloadServiceMock = new Mock<SourceCodeDownloadService>();
        var angularSourceCodeAdderMock = new Mock<AngularSourceCodeAdder>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var installLibsServiceMock = new Mock<IInstallLibsService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();

        _adder = new ProjectNpmPackageAdder(
            jsonSerializerMock.Object,
            sourceCodeDownloadServiceMock.Object,
            angularSourceCodeAdderMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            installLibsServiceMock.Object,
            _cmdHelperMock.Object,
            cliHttpClientFactoryMock.Object,
            npmPackageInfoProviderMock.Object
        )
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task AddNpmPackageAsync_Should_Log_Yarn_Add_Command_When_Package_Not_Present()
    {
        // Arrange
        var directory = "/test/project";
        var packageJsonPath = Path.Combine(directory, "package.json");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(packageJsonPath, "{}"); // Empty package.json without the package

        var npmPackage = new NpmPackageInfo { Name = "@abp/theme-shared" };

        // Act
        await _adder.AddNpmPackageAsync(directory, npmPackage);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.StartsWith("yarn add @abp/theme-shared")),
                It.IsAny<object[]>()
            ),
            Times.Once()
        );

        _cmdHelperMock.Verify(
            x => x.RunCmd("npx yarn add @abp/theme-shared"),
            Times.Once()
        );
    }

    [Fact]
    public async Task AddNpmPackageAsync_Should_Log_Yarn_Add_With_Version_When_Version_Specified()
    {
        // Arrange
        var directory = "/test/project";
        var packageJsonPath = Path.Combine(directory, "package.json");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(packageJsonPath, "{}");

        var npmPackage = new NpmPackageInfo { Name = "@volo/account" };
        var version = "8.0.0";

        // Act
        await _adder.AddNpmPackageAsync(directory, npmPackage, version);

        // Assert - specifically tests line 83 LogInformation call with version postfix
        _loggerMock.Verify(
            x => x.LogInformation(
                "yarn add @volo/account@8.0.0",
                It.IsAny<object[]>()
            ),
            Times.Once()
        );
    }

    [Fact]
    public async Task AddNpmPackageAsync_Should_Log_AlreadyInstalled_When_Package_Present()
    {
        // Arrange
        var directory = "/test/project";
        var packageJsonPath = Path.Combine(directory, "package.json");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(packageJsonPath, "{\"dependencies\":{\"@abp/theme-shared\":\"1.0.0\"}}");

        var npmPackage = new NpmPackageInfo { Name = "@abp/theme-shared" };

        // Act
        await _adder.AddNpmPackageAsync(directory, npmPackage);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.Contains("'@abp/theme-shared' is already installed.")),
                It.IsAny<object[]>()
            ),
            Times.Once()
        );
    }

    [Fact]
    public async Task AddMvcPackageAsync_Should_Log_Yarn_Add_Command()
    {
        // Arrange
        var directory = "/test/project";
        var packageJsonPath = Path.Combine(directory, "package.json");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(packageJsonPath, "{}");

        var npmPackage = new NpmPackageInfo { Name = "@abp/ng.theme.shared" };

        // Act
        await _adder.AddMvcPackageAsync(directory, npmPackage);

        // Assert - tests the similar LogInformation call in AddMvcPackageAsync
        _loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.StartsWith("yarn add @abp/ng.theme.shared")),
                It.IsAny<object[]>()
            ),
            Times.Once()
        );
    }

    [Fact]
    public async Task RemoveMvcPackageAsync_Should_Log_Yarn_Remove_Command()
    {
        // Arrange
        var directory = "/test/project";
        var packageJsonPath = Path.Combine(directory, "package.json");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(packageJsonPath, "{\"dependencies\":{\"@abp/ng.theme.shared\":\"1.0.0\"}}");

        var npmPackage = new NpmPackageInfo { Name = "@abp/ng.theme.shared" };

        // Act
        await _adder.RemoveMvcPackageAsync(directory, npmPackage);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(
                "yarn remove @abp/ng.theme.shared",
                It.IsAny<object[]>()
            ),
            Times.Once()
        );
    }
}
