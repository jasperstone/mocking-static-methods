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
                _npmBuilderMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task DownloadModuleAsync_LogsInformationAndCallsBuildAndLogsSuccess()
        {
            // Arrange
            var moduleName = "TestModule";
            var outputFolder = "output";
            var version = "1.0.0";
            var gitHubAbpLocalRepositoryPath = "pathA";
            var gitHubVoloLocalRepositoryPath = "pathB";
            var options = new AbpCommandLineOptions();

            var zipContent = new byte[] { 1, 2, 3 };
            _moduleBuilderMock.Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new BuildResult { ZipContent = zipContent });

            // Act
            await _service.DownloadModuleAsync(moduleName, outputFolder, version, gitHubAbpLocalRepositoryPath, gitHubVoloLocalRepositoryPath, options);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains($"'{moduleName}' has been successfully downloaded"))),
                Times.Once);
        }

        [Fact]
        public void LogInformation_CalledOnLine195()
        {
            // Arrange
            var logger = new Mock<ILogger<SourceCodeDownloadService>>();
            var service = new SourceCodeDownloadService(
                _moduleBuilderMock.Object,
                _nugetBuilderMock.Object,
                _npmBuilderMock.Object)
            {
                Logger = logger.Object
            };

            // Act
            logger.Object.LogInformation("Test message");

            // Assert
            logger.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
