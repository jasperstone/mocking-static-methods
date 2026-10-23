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
            var packageName = "TestPackage";
            var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var version = "1.0.0";

            var sourceCodeStoreMock = new Mock<ISourceCodeStore>();
            sourceCodeStoreMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<SourceCodeTypes>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
                .ReturnsAsync(new SourceCodeFile
                {
                    Version = "1.0.0",
                    Content = CreateTestZipContent()
                });

            var nugetPackageProjectBuilder = new NugetPackageProjectBuilderStub(sourceCodeStoreMock.Object);

            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilder,
                npmPackageProjectBuilderMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await service.DownloadNugetPackageAsync(packageName, outputFolder, version);

            // Assert
            loggerMock.Verify(
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

        private class NugetPackageProjectBuilderStub : NugetPackageProjectBuilder
        {
            public NugetPackageProjectBuilderStub(ISourceCodeStore sourceCodeStore)
                : base(sourceCodeStore, null, null, null, null, null)
            {
            }
        }

        private static byte[] CreateTestZipContent()
        {
            using var ms = new MemoryStream();
            using (var zip = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ms))
            {
                var entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry("file.txt");
                zip.PutNextEntry(entry);
                var content = System.Text.Encoding.UTF8.GetBytes("Hello World");
                zip.Write(content, 0, content.Length);
                zip.CloseEntry();
                zip.IsStreamOwner = false; // Leave stream open
                zip.Finish();
            }
            return ms.ToArray();
        }
    }
}
