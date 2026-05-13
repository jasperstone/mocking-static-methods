using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services;

public class SourceCodeDownloadServiceTests
{
    [Fact]
    public async Task DownloadNugetPackageAsync_LogsInformationAfterDownload()
    {
        // Arrange
        var packageName = "TestPackage";
        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var version = "1.0.0";

        // Create a simple zip archive in memory with one file
        var zipContent = CreateZipArchive(new[] { ("file.txt", "Hello World") });

        var mockNugetPackageProjectBuilder = new Mock<NugetPackageProjectBuilder>(null, null, null);
        mockNugetPackageProjectBuilder
            .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(zipContent, packageName));

        var mockModuleProjectBuilder = new Mock<ModuleProjectBuilder>(null, null, null);
        var mockNpmPackageProjectBuilder = new Mock<NpmPackageProjectBuilder>(null, null, null);

        var service = new SourceCodeDownloadService(
            mockModuleProjectBuilder.Object,
            mockNugetPackageProjectBuilder.Object,
            mockNpmPackageProjectBuilder.Object);

        var mockLogger = new Mock<ILogger<SourceCodeDownloadService>>();
        service.Logger = mockLogger.Object;

        // Act
        await service.DownloadNugetPackageAsync(packageName, outputFolder, version);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{packageName}' has been successfully downloaded to '{outputFolder}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Cleanup
        if (Directory.Exists(outputFolder))
        {
            Directory.Delete(outputFolder, true);
        }
    }

    private static byte[] CreateZipArchive((string fileName, string content)[] files)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var (fileName, content) in files)
            {
                var entry = archive.CreateEntry(fileName);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream, Encoding.UTF8);
                writer.Write(content);
            }
        }
        return ms.ToArray();
    }
}
