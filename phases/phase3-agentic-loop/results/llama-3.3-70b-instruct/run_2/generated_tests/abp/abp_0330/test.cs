using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_UsesLocalTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions();
            var jsonSerializer = new Volo.Abp.Json.JsonSerializer();
            var remoteServiceExceptionHandler = new Volo.Abp.Cli.ProjectBuilding.RemoteServiceExceptionHandler(jsonSerializer);
            var cliHttpClientFactory = new Volo.Abp.Cli.Http.CliHttpClientFactory();
            var cliVersionService = new Volo.Abp.Cli.Version.CliVersionService();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(options),
                jsonSerializer,
                remoteServiceExceptionHandler,
                cliHttpClientFactory,
                cliVersionService);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateSource = Path.Combine(Directory.GetCurrentDirectory(), "template-source");
            var templateName = "template-name";
            var version = "1.0.0";

            Directory.CreateDirectory(templateSource);
            File.Create(Path.Combine(templateSource, $"{templateName}-{version}.zip")).Dispose();

            // Act
            await abpIoSourceCodeStore.GetAsync(templateName, "template", version, templateSource);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Using local template: {templateName}, version: {version}"))),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_UsesCachedTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions { CacheTemplates = true };
            var jsonSerializer = new Volo.Abp.Json.JsonSerializer();
            var remoteServiceExceptionHandler = new Volo.Abp.Cli.ProjectBuilding.RemoteServiceExceptionHandler(jsonSerializer);
            var cliHttpClientFactory = new Volo.Abp.Cli.Http.CliHttpClientFactory();
            var cliVersionService = new Volo.Abp.Cli.Version.CliVersionService();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(options),
                jsonSerializer,
                remoteServiceExceptionHandler,
                cliHttpClientFactory,
                cliVersionService);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateName = "template-name";
            var version = "1.0.0";

            var localCacheFile = Path.Combine(Volo.Abp.Cli.CliPaths.TemplateCache, $"{templateName.Replace("/", ".")}-{version}.zip");
            Directory.CreateDirectory(Path.GetDirectoryName(localCacheFile));
            File.Create(localCacheFile).Dispose();

            // Act
            await abpIoSourceCodeStore.GetAsync(templateName, "template", version);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Using cached template: {templateName}, version: {version}"))),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_DownloadsTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions();
            var jsonSerializer = new Volo.Abp.Json.JsonSerializer();
            var remoteServiceExceptionHandler = new Volo.Abp.Cli.ProjectBuilding.RemoteServiceExceptionHandler(jsonSerializer);
            var cliHttpClientFactory = new Volo.Abp.Cli.Http.CliHttpClientFactory();
            var cliVersionService = new Volo.Abp.Cli.Version.CliVersionService();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(options),
                jsonSerializer,
                remoteServiceExceptionHandler,
                cliHttpClientFactory,
                cliVersionService);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateName = "template-name";
            var version = "1.0.0";

            // Act
            await abpIoSourceCodeStore.GetAsync(templateName, "template", version);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Downloading template: {templateName}, version: {version}"))),
                Times.Once);
        }
    }
}
