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
            var jsonSerializer = new JsonSerializationHelper();
            var remoteServiceExceptionHandler = new RemoteServiceExceptionHandler();
            var cancellationTokenProvider = new CancellationTokenProvider();
            var cliHttpClientFactory = new CliHttpClientFactory();
            var cliVersionService = new CliVersionService();
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new OptionsWrapper<AbpCliOptions>(options),
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactory,
                cliVersionService
            );
            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateSource = Path.Combine(Directory.GetCurrentDirectory(), "template-source");
            var templateName = "my-template";
            var version = "1.0.0";

            // Act
            await abpIoSourceCodeStore.GetAsync(templateName, SourceCodeTypes.Template, version, templateSource);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Using local {SourceCodeTypes.Template}: {templateName}, version: {version}"))
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAsync_UsesCachedTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions { CacheTemplates = true };
            var jsonSerializer = new JsonSerializationHelper();
            var remoteServiceExceptionHandler = new RemoteServiceExceptionHandler();
            var cancellationTokenProvider = new CancellationTokenProvider();
            var cliHttpClientFactory = new CliHttpClientFactory();
            var cliVersionService = new CliVersionService();
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new OptionsWrapper<AbpCliOptions>(options),
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactory,
                cliVersionService
            );
            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateSource = Path.Combine(Directory.GetCurrentDirectory(), "template-source");
            var templateName = "my-template";
            var version = "1.0.0";

            // Act
            await abpIoSourceCodeStore.GetAsync(templateName, SourceCodeTypes.Template, version, templateSource, skipCache: false);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Using cached {SourceCodeTypes.Template}: {templateName}, version: {version}"))
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAsync_DownloadsTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions();
            var jsonSerializer = new JsonSerializationHelper();
            var remoteServiceExceptionHandler = new RemoteServiceExceptionHandler();
            var cancellationTokenProvider = new CancellationTokenProvider();
            var cliHttpClientFactory = new CliHttpClientFactory();
            var cliVersionService = new CliVersionService();
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new OptionsWrapper<AbpCliOptions>(options),
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactory,
                cliVersionService
            );
            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateName = "my-template";
            var version = "1.0.0";

            // Act
            await abpIoSourceCodeStore.GetAsync(templateName, SourceCodeTypes.Template, version);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Downloading {SourceCodeTypes.Template}: {templateName}, version: {version}"))
                ),
                Times.Once
            );
        }
    }
}
