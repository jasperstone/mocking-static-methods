using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task GetAsync_Should_LogInformation_When_Using_Local_Non_Network_Source()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, eventId, state, ex, formatter) => 
            {
                if (level == LogLevel.Information)
                {
                    loggerMock.Object.Log(level, eventId, state, ex, formatter);
                }
            });

        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            Mock.Of<Volo.Abp.Json.IJsonSerializer>(),
            Mock.Of<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
            Mock.Of<Volo.Abp.Cli.Version.CliVersionService>()
        );
        store.Logger = loggerMock.Object;

        var templateSource = "/local/path"; // Local path, not network
        var name = "test-template";
        var type = "template";
        var version = "1.0.0";

        // Mock dependencies to reach the logging line without throwing
        var jsonSerializerMock = Mock.Of<Volo.Abp.Json.IJsonSerializer>();
        var remoteHandlerMock = Mock.Of<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>();
        var cancellationMock = Mock.Of<ICancellationTokenProvider>();
        var httpFactoryMock = Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>();
        var cliVersionMock = new Mock<Volo.Abp.Cli.Version.CliVersionService>();
        cliVersionMock.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(new Volo.Abp.Cli.Version.SemanticVersion(1, 0, 0));

        // Use Moq.Protected to mock internal calls or create a testable version
        // For this test, we focus on verifying the logger call pattern

        // Act
        await store.GetAsync(name, type, version, templateSource);

        // Assert - verify LogInformation was called
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Using local") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
