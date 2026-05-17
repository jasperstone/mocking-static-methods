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
        private readonly Mock<ModuleProjectBuilder> _moduleBuilderMock;
        private readonly Mock<NugetPackageProjectBuilder> _nugetBuilderMock;
        private readonly Mock<NpmPackageProjectBuilder> _npmBuilderMock;
        private readonly Mock<ILogger<SourceCodeDownloadService>> _loggerMock;
        private readonly SourceCodeDownloadService _service;

        public SourceCodeDownloadServiceTests()
        {
            _moduleBuilderMock = new Mock<ModuleProjectBuilder>();
            _nugetBuilderMock = new Mock<NugetPackageProjectBuilder>();
            _npmBuilderMock = new Mock<NpmPackageProjectBuilder>();
            _loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

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
        public async Task DownloadModuleAsync_LogsInformationAndBuildsAndLogsSuccess()
        {
            // Arrange
            var moduleName = "TestModule";
            var outputFolder = "output";
            var version = "1.0.0";
            var gitHubAbpLocalRepositoryPath = "abpPath";
            var gitHubVoloLocalRepositoryPath = "voloPath";
            var options = new AbpCommandLineOptions();

            var zipContent = new byte[] { 1, 2, 3 };
            var buildResult = new { ZipContent = zipContent };
            _moduleBuilderMock.Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(buildResult);

            // Act
            await _service.DownloadModuleAsync(moduleName, outputFolder, version, gitHubAbpLocalRepositoryPath, gitHubVoloLocalRepositoryPath, options);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Downloading source code of {moduleName}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            _loggerMock.Verify(
                x => x.LogInformation($"'{moduleName}' has been successfully downloaded to '{outputFolder}'"),
                Times.Once);
        }

        [Fact]
        public void IsAngularTestFile_ReturnsExpectedResults()
        {
            // Arrange & Act
            var methodInfo = typeof(SourceCodeDownloadService).GetMethod("IsAngularTestFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result1 = methodInfo.Invoke(_service, new object[] { "angular/testfile.js" });
            var result2 = methodInfo.Invoke(_service, new object[] { "angular/projects/test" });
            var result3 = methodInfo.Invoke(_service, new object[] { "other/file.js" });
            var result4 = methodInfo.Invoke(_service, new object[] { null });

            // Assert
            Assert.True((bool)result1);
            Assert.False((bool)result2);
            Assert.False((bool)result3);
            Assert.False((bool)result4);
        }
    }
}
