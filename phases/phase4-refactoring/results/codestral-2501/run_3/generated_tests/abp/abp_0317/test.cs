using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task GetAsync_RemoteServiceUnavailable_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProviderMock.Object,
            cliHttpClientFactoryMock.Object,
            cliVersionServiceMock.Object
        );

        store.Logger = loggerMock.Object;

        // Act
        await Assert.ThrowsAsync<CliUsageException>(() => store.GetAsync("TestTemplate", "TestType"));

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The remote service is currently unavailable, please specify the version.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
