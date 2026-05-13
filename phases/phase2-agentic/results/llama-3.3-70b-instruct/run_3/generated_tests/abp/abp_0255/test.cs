using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadModuleAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var sourceCodeDownloadService = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            sourceCodeDownloadService.Logger = loggerMock.Object;

            // Act
            await sourceCodeDownloadService.DownloadModuleAsync(
                "moduleName",
                "outputFolder",
                "version",
                "gitHubAbpLocalRepositoryPath",
                "gitHubVoloLocalRepositoryPath",
                new AbpCommandLineOptions()
            );

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("moduleName") && s.Contains("outputFolder"))),
                Times.Once
            );
        }

        [Fact]
        public async Task DownloadNugetPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var sourceCodeDownloadService = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            sourceCodeDownloadService.Logger = loggerMock.Object;

            // Act
            await sourceCodeDownloadService.DownloadNugetPackageAsync(
                "packageName",
                "outputFolder",
                "version"
            );

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("packageName") && s.Contains("outputFolder"))),
                Times.Once
            );
        }

        [Fact]
        public async Task DownloadNpmPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var sourceCodeDownloadService = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            sourceCodeDownloadService.Logger = loggerMock.Object;

            // Act
            await sourceCodeDownloadService.DownloadNpmPackageAsync(
                "packageName",
                "outputFolder",
                "version"
            );

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("packageName") && s.Contains("outputFolder"))),
                Times.Once
            );
        }
    }
}
