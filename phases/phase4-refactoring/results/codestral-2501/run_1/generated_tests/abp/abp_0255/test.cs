using System;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadModuleAsync_ShouldLogSuccessMessage()
        {
            // Arrange
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            service.Logger = loggerMock.Object;

            var moduleName = "TestModule";
            var outputFolder = "TestOutputFolder";
            var version = "1.0.0";
            var gitHubAbpLocalRepositoryPath = "TestAbpPath";
            var gitHubVoloLocalRepositoryPath = "TestVoloPath";
            var options = new AbpCommandLineOptions();

            var projectBuildResult = new ProjectBuildResult(new byte[0], "TestProject");

            moduleProjectBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(projectBuildResult);

            // Act
            await service.DownloadModuleAsync(moduleName, outputFolder, version, gitHubAbpLocalRepositoryPath, gitHubVoloLocalRepositoryPath, options);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{moduleName}' has been successfully downloaded to '{outputFolder}'")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public async Task DownloadNugetPackageAsync_ShouldLogSuccessMessage()
        {
            // Arrange
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            service.Logger = loggerMock.Object;

            var packageName = "TestPackage";
            var outputFolder = "TestOutputFolder";
            var version = "1.0.0";

            var projectBuildResult = new ProjectBuildResult(new byte[0], "TestProject");

            nugetPackageProjectBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(projectBuildResult);

            // Act
            await service.DownloadNugetPackageAsync(packageName, outputFolder, version);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{packageName}' has been successfully downloaded to '{outputFolder}'")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public async Task DownloadNpmPackageAsync_ShouldLogSuccessMessage()
        {
            // Arrange
            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilderMock.Object,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );
            service.Logger = loggerMock.Object;

            var packageName = "TestPackage";
            var outputFolder = "TestOutputFolder";
            var version = "1.0.0";

            var projectBuildResult = new ProjectBuildResult(new byte[0], "TestProject");

            npmPackageProjectBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(projectBuildResult);

            // Act
            await service.DownloadNpmPackageAsync(packageName, outputFolder, version);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{packageName}' has been successfully downloaded to '{outputFolder}'")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
