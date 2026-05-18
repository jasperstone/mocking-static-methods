using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task GetAsync_LogsWarning_WhenRemoteServiceUnavailable()
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
            cliVersionServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync()).ReturnsAsync(new SemanticVersion(1, 0, 0));

        // Act
        await Assert.ThrowsAsync<CliUsageException>(() => store.GetAsync("templateName", "templateType"));

        // Assert
        loggerMock.Verify(
            l => l.LogWarning("The remote service is currently unavailable, please specify the version."),
            Times.Once);
    }
}
