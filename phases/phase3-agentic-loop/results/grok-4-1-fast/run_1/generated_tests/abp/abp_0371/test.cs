using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectModification;

public class ProjectNpmPackageAdderTests
{
    private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
    private readonly ProjectNpmPackageAdder _adder;

    public ProjectNpmPackageAdderTests()
    {
        _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
        _loggerMock.SetupAllProperties();

        // Mock only the dependencies that are actually called in the test paths
        // Use Moq proxies for concrete types and minimal mocks for interfaces
        var jsonSerializerMock = new Mock<object>().Object;
        var sourceCodeDownloadServiceMock = new Mock<object>().Object;
        var angularSourceCodeAdderMock = new Mock<object>().Object;
        
        // Create minimal mocks for interfaces using Moq
        var remoteServiceExceptionHandlerMock = new Mock<object>().Object;
        var installLibsServiceMock = new Mock<object>().Object;
        var cmdHelperMock = new Mock<object>().Object;
        var cliHttpClientFactoryMock = new Mock<object>().Object;
        var npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();
        npmPackageInfoProviderMock.Setup(x => x.GetPackageListAsync()).ReturnsAsync(new System.Collections.Generic.List<NpmPackageInfo>());

        _adder = new ProjectNpmPackageAdder(
            jsonSerializerMock,
            sourceCodeDownloadServiceMock,
            angularSourceCodeAdderMock,
            remoteServiceExceptionHandlerMock,
            installLibsServiceMock,
            cmdHelperMock,
            cliHttpClientFactoryMock,
            npmPackageInfoProviderMock.Object
        )
        {
            Logger = _loggerMock.Object,
            CmdHelper = Mock.Of<ICmdHelper>() // Minimal ICmdHelper to avoid null issues
        };
    }

    [Fact]
    public async Task AddNpmPackageAsync_LogsYarnAddCommand_WhenPackageNotInstalled()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), "test-project");
        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonPath, "{}");

            // Act
            await _adder.AddNpmPackageAsync(directory, new NpmPackageInfo { Name = "@abp/theme-shared" });

            // Assert - Verifies LogInformation call on line 83
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v?.ToString()).Contains("yarn add @abp/theme-shared")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                try { Directory.Delete(directory, true); } catch { }
            }
        }
    }

    [Fact]
    public async Task AddNpmPackageAsync_LogsYarnAddWithVersion_WhenVersionProvided()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), "test-project-v");
        try
        {
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonPath, "{}");

            // Act
            await _adder.AddNpmPackageAsync(directory, new NpmPackageInfo { Name = "@abp/my-package" }, "1.2.3");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v?.ToString()).Contains("yarn add @abp/my-package@1.2.3")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                try { Directory.Delete(directory, true); } catch { }
            }
        }
    }
}
