using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _store;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
            var options = Options.Create(new AbpCliOptions());
            _store = new AbpIoSourceCodeStore(
                options,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<CliHttpClientFactory>().Object,
                _cliVersionServiceMock.Object);
            _store.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_Should_LogWarning_When_LatestVersionIsNull()
        {
            // Arrange
            var store = _store;
            var logger = _loggerMock.Object;
            var name = "test";
            var type = "template";

            // Mock GetLatestSourceCodeVersionAsync to return null
            var storeType = typeof(AbpIoSourceCodeStore);
            var method = storeType.GetMethod("GetAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            // We can't directly mock internal methods, so we simulate the scenario by calling with version=null and mocking dependencies

            // Act
            await Assert.ThrowsAsync<CliUsageException>(async () =>
            {
                await store.GetAsync(name, type);
            });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The remote service is currently unavailable")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
