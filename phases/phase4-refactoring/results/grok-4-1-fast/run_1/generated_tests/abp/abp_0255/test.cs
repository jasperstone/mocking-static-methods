using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests;

public class SourceCodeDownloadServiceTests
{
    private readonly Mock<ModuleProjectBuilder> _moduleBuilderMock;
    private readonly Mock<NugetPackageProjectBuilder> _nugetBuilderMock;
    private readonly Mock<NpmPackageProjectBuilder> _npmBuilderMock;
    private readonly Mock<ILogger<SourceCodeDownloadService>> _loggerMock;

    public SourceCodeDownloadServiceTests()
    {
        _moduleBuilderMock = new Mock<ModuleProjectBuilder>();
        _nugetBuilderMock = new Mock<NugetPackageProjectBuilder>();
        _npmBuilderMock = new Mock<NpmPackageProjectBuilder>();
        _loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
    }

    [Fact]
    public async Task DownloadModuleAsync_Should_Log_Success_Message()
    {
        // Arrange
        _moduleBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(() => new byte[] { 1, 2, 3 });

        var service = new SourceCodeDownloadService(
            _moduleBuilderMock.Object,
            _nugetBuilderMock.Object,
            _npmBuilderMock.Object
        )
        {
            Logger = _loggerMock.Object
        };

        // Act
        await service.DownloadModuleAsync(
            "TestModule",
            "/output/folder",
            "1.0.0",
            null,
            null,
            null
        );

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("'TestModule' has been successfully downloaded to '/output/folder'") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DownloadNugetPackageAsync_Should_Log_Success_Message()
    {
        // Arrange
        _nugetBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(() => new byte[] { 1, 2, 3 });

        var service = new SourceCodeDownloadService(
            _moduleBuilderMock.Object,
            _nugetBuilderMock.Object,
            _npmBuilderMock.Object
        )
        {
            Logger = _loggerMock.Object
        };

        // Act
        await service.DownloadNugetPackageAsync(
            "Test.Package",
            "/output/folder",
            "1.0.0"
        );

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("'Test.Package' has been successfully downloaded to '/output/folder'") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DownloadNpmPackageAsync_Should_Log_Success_Message()
    {
        // Arrange
        _npmBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(() => new byte[] { 1, 2, 3 });

        var service = new SourceCodeDownloadService(
            _moduleBuilderMock.Object,
            _nugetBuilderMock.Object,
            _npmBuilderMock.Object
        )
        {
            Logger = _loggerMock.Object
        };

        // Act
        await service.DownloadNpmPackageAsync(
            "@abp/npm-package",
            "/output/folder",
            "1.0.0"
        );

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("'@abp/npm-package' has been successfully downloaded to '/output/folder'") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
