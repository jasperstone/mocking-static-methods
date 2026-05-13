using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services;

public class SourceCodeDownloadServiceTests
{
    private static byte[] CreateZipBytes(params (string EntryName, string Content)[] entries)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var (entryName, content) in entries)
            {
                var entry = archive.CreateEntry(entryName);
                using var entryStream = entry.Open();
                var bytes = Encoding.UTF8.GetBytes(content);
                entryStream.Write(bytes, 0, bytes.Length);
            }
        }
        return ms.ToArray();
    }

    [Fact]
    public async Task DownloadNugetPackageAsync_LogsInformationAfterDownload()
    {
        // Arrange
        var packageName = "TestPackage";
        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputFolder);
        var version = "1.0.0";

        var zipBytes = CreateZipBytes(
            ("file1.txt", "Hello World"),
            ("folder/file2.txt", "Content")
        );

        var mockNugetBuilder = new Mock<NugetPackageProjectBuilder>();
        mockNugetBuilder
            .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ProjectBuildResult(zipBytes, packageName));

        var mockLogger = new Mock<ILogger<SourceCodeDownloadService>>();

        var service = new SourceCodeDownloadService(
            moduleProjectBuilder: null!,
            nugetPackageProjectBuilder: mockNugetBuilder.Object,
            npmPackageProjectBuilder: null!
        )
        {
            Logger = mockLogger.Object
        };

        // Act
        await service.DownloadNugetPackageAsync(packageName, outputFolder, version);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"'{packageName}' has been successfully downloaded to '{outputFolder}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        if (Directory.Exists(outputFolder))
        {
            Directory.Delete(outputFolder, true);
        }
    }
}
