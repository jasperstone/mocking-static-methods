using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<CmdHelper> _cmdHelperMock;
        private readonly AbpIoSourceCodeStore _store;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _optionsMock = new Mock<IOptions<AbpCliOptions>>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _cmdHelperMock = new Mock<CmdHelper>();

            var cliHttpClientFactory = new CliHttpClientFactory(_httpClientFactoryMock.Object, _cancellationTokenProviderMock.Object);
            var cliVersionService = new CliVersionService(_cmdHelperMock.Object);

            _store = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                cliHttpClientFactory,
                cliVersionService
            );
            _store.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_ShouldLogInformation_WhenUsingLocalTemplate()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = "localSource";
            var expectedLogMessage = "Using local Template: TestTemplate, version: 1.0.0";

            // Act
            await _store.GetAsync(name, type, version, templateSource);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(expectedLogMessage),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAsync_ShouldLogInformation_WhenUsingCachedTemplate()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var expectedLogMessage = "Using cached Template: TestTemplate, version: 1.0.0";

            // Act
            await _store.GetAsync(name, type, version);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(expectedLogMessage),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAsync_ShouldLogInformation_WhenDownloadingTemplate()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var expectedLogMessage = "Downloading Template: TestTemplate, version: 1.0.0";

            // Act
            await _store.GetAsync(name, type, version);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(expectedLogMessage),
                Times.Once
            );
        }
    }
}
