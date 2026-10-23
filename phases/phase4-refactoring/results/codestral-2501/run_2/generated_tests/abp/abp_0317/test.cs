using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Threading;
using Xunit;
using Volo.Abp.Cli.Http;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task GetAsync_RemoteServiceUnavailable_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        var store = new Mock<AbpIoSourceCodeStore>(
            null,
            null,
            null,
            cancellationTokenProviderMock.Object,
            cliHttpClientFactoryMock.Object,
            cliVersionServiceMock.Object)
        {
            CallBase = true
        };

        store.Object.Logger = loggerMock.Object;

        // Mock the GetAsync method to simulate the scenario where the remote service is unavailable
        store.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("The remote service is currently unavailable, please specify the version."));

        // Act
        await Assert.ThrowsAsync<Exception>(() => store.Object.GetAsync("templateName", "templateType"));

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(4));
    }
}
