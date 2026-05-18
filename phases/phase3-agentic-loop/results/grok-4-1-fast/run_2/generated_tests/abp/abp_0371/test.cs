using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests;

public class ProjectNpmPackageAdderTests
{
    private readonly Mock<ILogger<ProjectNpmPackageAdder>> _mockLogger;

    public ProjectNpmPackageAdderTests()
    {
        _mockLogger = new Mock<ILogger<ProjectNpmPackageAdder>>();
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogYarnAddCommand_Line83_WhenPackageNotInstalled()
    {
        // Arrange - Create temp directory with empty package.json to hit the yarn add branch (line 83)
        var directory = Path.Combine(Path.GetTempPath(), "test-project-npm-adder");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "package.json"), "{}");
        
        var npmPackage = new NpmPackageInfo { Name = "@abp/ng.theme.shared" };

        // Create minimal mocks only for constructor - we don't call their methods
        var mockJsonSerializer = new Mock<IJsonSerializer>().Object;
        var mockSourceCodeDownloadService = new Mock<SourceCodeDownloadService>().Object;
        var mockAngularSourceCodeAdder = new Mock<AngularSourceCodeAdder>().Object;
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>().Object;
        var mockInstallLibsService = new Mock<IInstallLibsService>().Object;
        var mockCmdHelper = new Mock<ICmdHelper>();
        mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>())).Returns(0);
        var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>().Object;
        var mockNpmPackageInfoProvider = new Mock<INpmPackageInfoProvider>().Object;

        var adder = new ProjectNpmPackageAdder(
            mockJsonSerializer,
            mockSourceCodeDownloadService,
            mockAngularSourceCodeAdder,
            mockRemoteServiceExceptionHandler,
            mockInstallLibsService,
            mockCmdHelper.Object,
            mockCliHttpClientFactory.Object,
            mockNpmPackageInfoProvider.Object
        )
        {
            Logger = _mockLogger.Object
        };

        // Act
        await adder.AddNpmPackageAsync(directory, npmPackage);

        // Assert - Verify the EXACT LogInformation call on line 83
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg == "yarn add @abp/ng.theme.shared")
            ),
            Times.Once()
        );

        // Cleanup
        Directory.Delete(directory, true);
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogYarnAddWithVersion_Line83_WhenVersionSpecified()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), "test-project-npm-version");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "package.json"), "{}");
        
        var npmPackage = new NpmPackageInfo { Name = "@abp/ng.theme.shared" };
        var version = "1.2.3";

        var mockJsonSerializer = new Mock<IJsonSerializer>().Object;
        var mockSourceCodeDownloadService = new Mock<SourceCodeDownloadService>().Object;
        var mockAngularSourceCodeAdder = new Mock<AngularSourceCodeAdder>().Object;
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>().Object;
        var mockInstallLibsService = new Mock<IInstallLibsService>().Object;
        var mockCmdHelper = new Mock<ICmdHelper>();
        mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>())).Returns(0);
        var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>().Object;
        var mockNpmPackageInfoProvider = new Mock<INpmPackageInfoProvider>().Object;

        var adder = new ProjectNpmPackageAdder(
            mockJsonSerializer,
            mockSourceCodeDownloadService,
            mockAngularSourceCodeAdder,
            mockRemoteServiceExceptionHandler,
            mockInstallLibsService,
            mockCmdHelper.Object,
            mockCliHttpClientFactory.Object,
            mockNpmPackageInfoProvider.Object
        )
        {
            Logger = _mockLogger.Object
        };

        // Act
        await adder.AddNpmPackageAsync(directory, npmPackage, version);

        // Assert - Verify line 83 LogInformation with version postfix
        _mockLogger.Verify(
            x => x.LogInformation("yarn add @abp/ng.theme.shared@1.2.3"),
            Times.Once()
        );

        Directory.Delete(directory, true);
    }

    [Fact]
    public async Task AddMvcPackageAsync_ShouldLogYarnAddCommand_SimilarPattern()
    {
        // Additional coverage for similar logging pattern
        var directory = Path.Combine(Path.GetTempPath(), "test-mvc-package");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "package.json"), "{\"dependencies\":{}}");
        
        var npmPackage = new NpmPackageInfo { Name = "@abp/ng.theme.shared" };

        var mockJsonSerializer = new Mock<IJsonSerializer>().Object;
        var mockSourceCodeDownloadService = new Mock<SourceCodeDownloadService>().Object;
        var mockAngularSourceCodeAdder = new Mock<AngularSourceCodeAdder>().Object;
        var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>().Object;
        var mockInstallLibsService = new Mock<IInstallLibsService>().Object;
        var mockCmdHelper = new Mock<ICmdHelper>();
        mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>())).Returns(0);
        var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>().Object;
        var mockNpmPackageInfoProvider = new Mock<INpmPackageInfoProvider>().Object;

        var adder = new ProjectNpmPackageAdder(
            mockJsonSerializer,
            mockSourceCodeDownloadService,
            mockAngularSourceCodeAdder,
            mockRemoteServiceExceptionHandler,
            mockInstallLibsService,
            mockCmdHelper.Object,
            mockCliHttpClientFactory.Object,
            mockNpmPackageInfoProvider.Object
        )
        {
            Logger = _mockLogger.Object
        };

        // Act
        await adder.AddMvcPackageAsync(directory, npmPackage);

        // Assert - Similar LogInformation pattern
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.StartsWith("yarn add @abp/ng.theme.shared"))
            ),
            Times.Once()
        );

        Directory.Delete(directory, true);
    }
}
