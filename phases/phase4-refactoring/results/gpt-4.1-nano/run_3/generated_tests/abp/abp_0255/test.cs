using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands.Services;

namespace Volo.Abp.Cli.Tests
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadModuleAsync_LogsSuccessMessage()
        {
            // Arrange
            var moduleBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var dummyZipContent = new byte[] { 1, 2, 3, 4 };
            var buildResult = new BuildResult { ZipContent = dummyZipContent };

            moduleBuilderMock.Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(buildResult);

            var service = new SourceCodeDownloadService(
                moduleBuilderMock.Object,
                nugetBuilderMock.Object,
                npmBuilderMock.Object)
            {
                Logger = loggerMock.Object
            };

            var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputFolder);

            // Act
            await service.DownloadModuleAsync(
                "TestModule",
                outputFolder,
                null,
                null,
                null,
                null);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("'TestModule' has been successfully downloaded"))),
                Times.Once);
        }
    }
}
