using System;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadNugetPackageAsync_LogsInformationAfterDownload()
        {
            // Arrange
            var packageName = "TestPackage";
            var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var version = "1.0.0";

            var zipContent = CreateZipWithSingleFile("file.txt", "Hello World");

            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>(MockBehavior.Strict);
            nugetPackageProjectBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult { ZipContent = zipContent });

            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(MockBehavior.Strict);
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>(MockBehavior.Strict);

            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await service.DownloadNugetPackageAsync(packageName, outputFolder, version);

            // Assert
            loggerMock.Verify(l => l.Log(
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

        private static byte[] CreateZipWithSingleFile(string fileName, string content)
        {
            using var ms = new MemoryStream();
            using (var zipOutputStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ms))
            {
                var entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(fileName)
                {
                    DateTime = DateTime.Now,
                    Size = content.Length
                };
                zipOutputStream.PutNextEntry(entry);
                var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                zipOutputStream.Write(buffer, 0, buffer.Length);
                zipOutputStream.CloseEntry();
                zipOutputStream.IsStreamOwner = false; // False stops the Close also closing the underlying stream.
                zipOutputStream.Finish();
            }
            return ms.ToArray();
        }
    }
}
