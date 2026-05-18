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
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _optionsMock = new Mock<IOptions<AbpCliOptions>>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
        }

        [Fact]
        public async Task GetAsync_UsesLocalTemplate_LogsInformation()
        {
            // Arrange
            var templateName = "template-name";
            var templateType = "template-type";
            var templateVersion = "template-version";
            var templateSource = "template-source";
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object
            );
            abpIoSourceCodeStore.Logger = _loggerMock.Object;

            // Act
            await abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                false,
                false,
                false
            );

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Using local {templateType}: {templateName}, version: {templateVersion}"))
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAsync_UsesCachedTemplate_LogsInformation()
        {
            // Arrange
            var templateName = "template-name";
            var templateType = "template-type";
            var templateVersion = "template-version";
            var templateSource = string.Empty;
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object
            );
            abpIoSourceCodeStore.Logger = _loggerMock.Object;
            var localCacheFile = Path.Combine("template-cache", templateName.Replace("/", ".") + "-" + templateVersion + ".zip");
            File.Create(localCacheFile).Dispose();

            // Act
            await abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                false,
                false,
                false
            );

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Using cached {templateType}: {templateName}, version: {templateVersion}"))
                ),
                Times.Once
            );

            File.Delete(localCacheFile);
        }

        [Fact]
        public async Task GetAsync_DownloadsTemplate_LogsInformation()
        {
            // Arrange
            var templateName = "template-name";
            var templateType = "template-type";
            var templateVersion = "template-version";
            var templateSource = string.Empty;
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object
            );
            abpIoSourceCodeStore.Logger = _loggerMock.Object;

            // Act
            await abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                false,
                true,
                false
            );

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Downloading {templateType}: {templateName}, version: {templateVersion}"))
                ),
                Times.Once
            );
        }
    }
}
