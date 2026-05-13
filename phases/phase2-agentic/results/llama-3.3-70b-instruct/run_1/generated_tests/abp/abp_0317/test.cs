using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_RemoteServiceUnavailable_LogsWarningAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<CliVersionService>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            // Act and Assert
            await Assert.ThrowsAsync<CliUsageException>(() => abpIoSourceCodeStore.GetAsync("name", "type"));
            loggerMock.Verify(l => l.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        }
    }
}
