using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadNugetPackageAsync_LogsInformationAfterDownload()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

            var zipContent = CreateTestZipContent();

            nugetPackageProjectBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectResult
                {
                    ZipContent = zipContent
                });

            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object)
            {
                Logger = loggerMock.Object
            };

            var packageName = "TestPackage";
            var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputFolder);

            try
            {
                // Act
                await service.DownloadNugetPackageAsync(packageName, outputFolder, "1.0.0");

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{packageName}' has been successfully downloaded to '{outputFolder}'")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true);
                }
            }
        }

        private static byte[] CreateTestZipContent()
        {
            using var ms = new MemoryStream();
            using (var zip = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ms))
            {
                zip.SetLevel(3);
                var entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry("file.txt")
                {
                    DateTime = DateTime.Now
                };
                zip.PutNextEntry(entry);
                var data = System.Text.Encoding.UTF8.GetBytes("Test content");
                zip.Write(data, 0, data.Length);
                zip.CloseEntry();
                zip.Finish();
            }
            return ms.ToArray();
        }
    }
}
