using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests;

public class SourceCodeDownloadServiceTests
{
    private readonly Mock<ILogger<SourceCodeDownloadService>> _loggerMock;
    private readonly Mock<ModuleProjectBuilder> _moduleBuilderMock;
    private readonly Mock<NugetPackageProjectBuilder> _nugetBuilderMock;
    private readonly Mock<NpmPackageProjectBuilder> _npmBuilderMock;
    private readonly SourceCodeDownloadService _service;

    public SourceCodeDownloadServiceTests()
    {
        _loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        _moduleBuilderMock = new Mock<ModuleProjectBuilder>();
        _nugetBuilderMock = new Mock<NugetPackageProjectBuilder>();
        _npmBuilderMock = new Mock<NpmPackageProjectBuilder>();

        _service = new SourceCodeDownloadService(
            _moduleBuilderMock.Object,
            _nugetBuilderMock.Object,
            _npmBuilderMock.Object)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task DownloadModuleAsync_Should_Log_Success_Message()
    {
        // Arrange
        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(outputFolder);

        var zipBytes = CreateMinimalZip();
        var result = new ProjectResult { ZipContent = zipBytes };
        _moduleBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(result);

        try
        {
            // Act
            await _service.DownloadModuleAsync("acme.bookstore", outputFolder, "1.0.0", null, null, null!);
        }
        finally
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'acme.bookstore' has been successfully downloaded to '{outputFolder}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DownloadNugetPackageAsync_Should_Log_Success_Message()
    {
        // Arrange
        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(outputFolder);

        var zipBytes = CreateMinimalZip();
        var result = new ProjectResult { ZipContent = zipBytes };
        _nugetBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(result);

        try
        {
            // Act
            await _service.DownloadNugetPackageAsync("Volo.Abp.Core", outputFolder, "1.0.0");
        }
        finally
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'Volo.Abp.Core' has been successfully downloaded to '{outputFolder}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DownloadNpmPackageAsync_Should_Log_Success_Message()
    {
        // Arrange
        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(outputFolder);

        var zipBytes = CreateMinimalZip();
        var result = new ProjectResult { ZipContent = zipBytes };
        _npmBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(result);

        try
        {
            // Act
            await _service.DownloadNpmPackageAsync("@abp/ng.theme.shared", outputFolder, "1.0.0");
        }
        finally
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"\'@abp/ng.theme.shared' has been successfully downloaded to '{outputFolder}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static byte[] CreateMinimalZip()
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var demoFile = archive.CreateEntry("demo.txt");
            using var entryStream = demoFile.Open();
            using var streamWriter = new StreamWriter(entryStream);
            streamWriter.Write("test");
        }
        return memoryStream.ToArray();
    }
}
