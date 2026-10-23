using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Cli.Http;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task GetAsync_LogsWarning_WhenLatestVersionIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        // We cannot find ICancellationTokenProvider type, so we use null for that parameter
        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProvider: null,
            cliHttpClientFactoryMock.Object,
            cliVersionServiceMock.Object);

        // Inject our logger mock
        store.Logger = loggerMock.Object;

        // Act
        // Call GetAsync with version = null and type that triggers the null latestVersion path
        var ex = await Assert.ThrowsAsync<CliUsageException>(() => store.GetAsync("TestTemplate", "templateType", null));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The remote service is currently unavailable, please specify the version.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Also verify some other warning logs were called (at least 1 more)
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeast(5));
    }
}
