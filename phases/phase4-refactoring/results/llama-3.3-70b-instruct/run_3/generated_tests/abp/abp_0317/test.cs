using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>> _optionsMock;
        private readonly Mock<Volo.Abp.Json.IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>();
            _jsonSerializerMock = new Mock<Volo.Abp.Json.IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
        }

        [Fact]
        public async Task GetAsync_WhenRemoteServiceIsUnavailable_LogsWarningAndThrowsException()
        {
            // Arrange
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);

            abpIoSourceCodeStore.Logger = _loggerMock.Object;

            // Act and Assert
            await Assert.ThrowsAsync<CliUsageException>(() => abpIoSourceCodeStore.GetAsync("name", "type"));
            _loggerMock.Verify(logger => logger.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        }
    }
}
