using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
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
            var moduleProjectBuilder = new ModuleProjectBuilder(
                new Mock<ISourceCodeStore>().Object,
                new Mock<IModuleInfoProvider>().Object,
                new Mock<ICliAnalyticsCollect>().Object,
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IApiKeyService>().Object
            );
            var nugetPackageProjectBuilder = new NugetPackageProjectBuilder(
                new Mock<ISourceCodeStore>().Object,
                new Mock<INugetPackageInfoProvider>().Object,
                new Mock<ICliAnalyticsCollect>().Object,
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IApiKeyService>().Object
            );
            var npmPackageProjectBuilder = new NpmPackageProjectBuilder(
                new Mock<ISourceCodeStore>().Object,
                new Mock<INpmPackageInfoProvider>().Object,
                new Mock<ICliAnalyticsCollect>().Object,
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IApiKeyService>().Object
            );
            var sourceCodeDownloadService = new SourceCodeDownloadService(
                moduleProjectBuilder,
                nugetPackageProjectBuilder,
                npmPackageProjectBuilder
            );
            sourceCodeDownloadService.Logger = loggerMock.Object;

            var moduleName = "MyModule";
            var outputFolder = "MyOutputFolder";
            var version = "1.0.0";

            var moduleProjectBuilderMock = new Mock<ModuleProjectBuilder>(new Mock<ISourceCodeStore>().Object,
                new Mock<IModuleInfoProvider>().Object,
                new Mock<ICliAnalyticsCollect>().Object,
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IApiKeyService>().Object);
            moduleProjectBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult(new byte[0], moduleName));

            // Act
            await sourceCodeDownloadService.DownloadModuleAsync(moduleName, outputFolder, version, null, null, null);

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains(moduleName) && s.Contains(outputFolder))), Times.Once);
        }
    }
}
