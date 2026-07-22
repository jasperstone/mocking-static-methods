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
        public async Task DownloadModuleAsync_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var moduleBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var service = new SourceCodeDownloadService(
                moduleBuilderMock.Object,
                nugetBuilderMock.Object,
                npmBuilderMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            var dummyZipContent = new byte[] { 1, 2, 3 };
            var buildResult = new BuildResult { ZipContent = dummyZipContent };
            moduleBuilderMock.Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(buildResult);

            // Act
            await service.DownloadModuleAsync(
                "TestModule",
                "output/path",
                "1.0.0",
                "gitHubAbpPath",
                "gitHubVoloPath",
                new AbpCommandLineOptions()
            );

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'TestModule' has been successfully downloaded to 'output/path'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
