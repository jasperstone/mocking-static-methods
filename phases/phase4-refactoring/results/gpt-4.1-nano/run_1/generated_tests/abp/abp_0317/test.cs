using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.IO;
using Microsoft.Extensions.Options;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_Should_LogWarning_When_LatestVersionIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = Options.Create(new AbpCliOptions());
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var store = new AbpIoSourceCodeStore(
                options,
                jsonSerializerMock.Object,
                remoteExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object);

            // Inject the logger
            store.Logger = loggerMock.Object;

            // Override GetLatestSourceCodeVersionAsync to return null
            var testStore = new TestAbpIoSourceCodeStore(
                options,
                jsonSerializerMock.Object,
                remoteExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object,
                latestVersionReturn: null);

            // Act
            await Assert.ThrowsAsync<CliUsageException>(() => testStore.GetAsync("name", "type"));

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The remote service is currently unavailable")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Helper subclass to override the private method
        private class TestAbpIoSourceCodeStore : AbpIoSourceCodeStore
        {
            private readonly string _latestVersionReturn;

            public TestAbpIoSourceCodeStore(
                IOptions<AbpCliOptions> options,
                IJsonSerializer jsonSerializer,
                IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
                ICancellationTokenProvider cancellationTokenProvider,
                CliHttpClientFactory cliHttpClientFactory,
                CliVersionService cliVersionService,
                string latestVersionReturn)
                : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
            {
                _latestVersionReturn = latestVersionReturn;
            }

            protected override Task<string> GetLatestSourceCodeVersionAsync(string name, string type, string param, bool includePreReleases)
            {
                return Task.FromResult(_latestVersionReturn);
            }
        }
    }
}
