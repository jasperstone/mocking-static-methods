using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
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
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var cliHttpClientFactory = new CliHttpClientFactory(httpClientFactoryMock.Object, cancellationTokenProviderMock.Object);

        // Mock CliVersionService with a dummy CmdHelper to avoid constructor issues
        var cmdHelperMock = new Mock<Volo.Abp.Cli.Utils.CmdHelper>();
        var cliVersionService = new CliVersionService(cmdHelperMock.Object);

        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProviderMock.Object,
            cliHttpClientFactory,
            cliVersionService);

        store.Logger = loggerMock.Object;

        // Act
        var exceptionThrown = false;
        try
        {
            await store.GetAsync("nonexistent-template", "template-type", null);
        }
        catch (CliUsageException)
        {
            exceptionThrown = true;
        }
        catch
        {
            // ignore other exceptions
        }

        // Assert
        Assert.True(exceptionThrown, "Expected CliUsageException was not thrown.");

        loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The remote service is currently unavailable, please specify the version.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
    }
}
