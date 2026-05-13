using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Volo.Abp.Http;
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
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _sourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _optionsMock = new Mock<IOptions<AbpCliOptions>>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _sourceCodeStore = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object
            );
            _sourceCodeStore.Logger = _loggerMock.Object;
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

            _cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(new NuGet.Versioning.SemanticVersion(1, 0, 0));
            _sourceCodeStore.Options.CacheTemplates = false;

            // Act
            await _sourceCodeStore.GetAsync(name, type, version, templateSource);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedLogMessage)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
