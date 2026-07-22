using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_RemoteServiceUnavailable_LogsWarning()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<AbpIoSourceCodeStore>>();
            var cancellationTokenProviderMock = Substitute.For<ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = Substitute.For<CliHttpClientFactory>();
            var cliVersionServiceMock = Substitute.For<CliVersionService>();
            var optionsMock = Substitute.For<IOptions<AbpCliOptions>>();
            var jsonSerializerMock = Substitute.For<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = Substitute.For<IRemoteServiceExceptionHandler>();

            var store = new AbpIoSourceCodeStore(
                optionsMock,
                jsonSerializerMock,
                remoteServiceExceptionHandlerMock,
                cancellationTokenProviderMock,
                cliHttpClientFactoryMock,
                cliVersionServiceMock
            );

            store.Logger = loggerMock;

            // Act
            await Assert.ThrowsAsync<Exception>(() => store.GetAsync("test", "testType"));

            // Assert
            loggerMock.Received(4).Log(
                Arg.Is<LogLevel>(l => l == LogLevel.Warning),
                Arg.Any<EventId>(),
                Arg.Any<It.IsAnyType>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<It.IsAnyType, Exception, string>>());
        }
    }
}
