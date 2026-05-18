using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests;

public class ProjectNpmPackageAdderTests
{
    private readonly Mock<ILogger<ProjectNpmPackageAdder>> _mockLogger;
    private readonly ProjectNpmPackageAdder _adder;

    public ProjectNpmPackageAdderTests()
    {
        _mockLogger = new Mock<ILogger<ProjectNpmPackageAdder>>();
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ));

        _adder = new TestableProjectNpmPackageAdder(
            new Mock<object>().Object, // IJsonSerializer
            new Mock<object>().Object, // SourceCodeDownloadService  
            new Mock<object>().Object, // AngularSourceCodeAdder
            new Mock<object>().Object, // IRemoteServiceExceptionHandler
            new Mock<object>().Object, // IInstallLibsService
            new Mock<object>().Object, // ICmdHelper
            new Mock<object>().Object, // CliHttpClientFactory
            new Mock<object>().Object  // INpmPackageInfoProvider
        )
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task AddNpmPackageAsync_ShouldLogYarnAddCommand_WhenPackageNotInstalled()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var npmPackage = new NpmPackageInfo { Name = "@abp/test-package" };
        var version = "1.0.0";
        var packageJsonPath = Path.Combine(directory, "package.json");

        Directory.CreateDirectory(directory);
        
        try
        {
            await File.WriteAllTextAsync(packageJsonPath, "{}");

            // Act
            await _adder.AddNpmPackageAsync(directory, npmPackage, version);

            // Assert - covers line 83 LogInformation("yarn add " + npmPackage.Name + versionPostfix)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state?.ToString()?.Contains("yarn add @abp/test-package@1.0.0") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}

public class TestableProjectNpmPackageAdder : ProjectNpmPackageAdder
{
    public TestableProjectNpmPackageAdder(
        object jsonSerializer,
        object sourceCodeDownloadService,
        object angularSourceCodeAdder,
        object remoteServiceExceptionHandler,
        object installLibsService,
        object cmdHelper,
        object cliHttpClientFactory,
        object npmPackageInfoProvider)
        : base(
            (global::Volo.Abp.Json.IJsonSerializer)jsonSerializer,
            (global::Volo.Abp.Cli.Commands.Services.SourceCodeDownloadService)sourceCodeDownloadService,
            (AngularSourceCodeAdder)angularSourceCodeAdder,
            (global::Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler)remoteServiceExceptionHandler,
            (global::Volo.Abp.Cli.LIbs.IInstallLibsService)installLibsService,
            (global::Volo.Abp.Cli.Utils.ICmdHelper)cmdHelper,
            (global::Volo.Abp.Cli.Http.CliHttpClientFactory)cliHttpClientFactory,
            (global::Volo.Abp.Cli.ProjectBuilding.INpmPackageInfoProvider)nmpPackageInfoProvider
        )
    {
    }
}
