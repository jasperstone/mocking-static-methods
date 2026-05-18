using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadModuleAsync_LogsSuccessMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var sourceCodeStoreMock = new Mock<ISourceCodeStore>();
            var moduleInfoProviderMock = new Mock<IModuleInfoProvider>();
            var cliAnalyticsCollectMock = new Mock<ICliAnalyticsCollect>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var apiKeyServiceMock = new Mock<IApiKeyService>();

            var moduleProjectBuilder = new ModuleProjectBuilder(
                sourceCodeStoreMock.Object,
                moduleInfoProviderMock.Object,
                cliAnalyticsCollectMock.Object,
                optionsMock.Object,
                jsonSerializerMock.Object,
                apiKeyServiceMock.Object
            );

            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilder,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );

            service.Logger = loggerMock.Object;

            // Act
            await service.DownloadModuleAsync("moduleName", "outputFolder", "version", null, null, null);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("moduleName has been successfully downloaded to 'outputFolder'"))),
                Times.Once
            );
        }

        [Fact]
        public async Task DownloadNugetPackageAsync_LogsSuccessMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var sourceCodeStoreMock = new Mock<ISourceCodeStore>();
            var moduleInfoProviderMock = new Mock<IModuleInfoProvider>();
            var cliAnalyticsCollectMock = new Mock<ICliAnalyticsCollect>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var apiKeyServiceMock = new Mock<IApiKeyService>();

            var moduleProjectBuilder = new ModuleProjectBuilder(
                sourceCodeStoreMock.Object,
                moduleInfoProviderMock.Object,
                cliAnalyticsCollectMock.Object,
                optionsMock.Object,
                jsonSerializerMock.Object,
                apiKeyServiceMock.Object
            );

            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilder,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );

            service.Logger = loggerMock.Object;

            // Act
            await service.DownloadNugetPackageAsync("packageName", "outputFolder", "version");

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("packageName has been successfully downloaded to 'outputFolder'"))),
                Times.Once
            );
        }

        [Fact]
        public async Task DownloadNpmPackageAsync_LogsSuccessMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
            var sourceCodeStoreMock = new Mock<ISourceCodeStore>();
            var moduleInfoProviderMock = new Mock<IModuleInfoProvider>();
            var cliAnalyticsCollectMock = new Mock<ICliAnalyticsCollect>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var apiKeyServiceMock = new Mock<IApiKeyService>();

            var moduleProjectBuilder = new ModuleProjectBuilder(
                sourceCodeStoreMock.Object,
                moduleInfoProviderMock.Object,
                cliAnalyticsCollectMock.Object,
                optionsMock.Object,
                jsonSerializerMock.Object,
                apiKeyServiceMock.Object
            );

            var nugetPackageProjectBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmPackageProjectBuilderMock = new Mock<NpmPackageProjectBuilder>();

            var service = new SourceCodeDownloadService(
                moduleProjectBuilder,
                nugetPackageProjectBuilderMock.Object,
                npmPackageProjectBuilderMock.Object
            );

            service.Logger = loggerMock.Object;

            // Act
            await service.DownloadNpmPackageAsync("packageName", "outputFolder", "version");

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("packageName has been successfully downloaded to 'outputFolder'"))),
                Times.Once
            );
        }
    }
}
