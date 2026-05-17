using System;
using System.Collections.Generic;
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
                _cliVersionServiceMock.Object
            );
            _store.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_Should_LogWarning_When_LatestVersionIsNull()
        {
            // Arrange
            var store = new TestAbpIoSourceCodeStore(
                Options.Create(new AbpCliOptions()),
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<CliHttpClientFactory>().Object,
                _cliVersionServiceMock.Object
            );
            store.Logger = _loggerMock.Object;
            store.GetLatestSourceCodeVersionAsyncFunc = (name, type, s, includePreReleases) => Task.FromResult<string>(null);

            // Act & Assert
            await Assert.ThrowsAsync<CliUsageException>(async () =>
            {
                await store.GetAsync("TestTemplate", "Template");
            });

            // Verify that the warning was logged
            _loggerMock.Verify(x => x.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        }

        // Additional tests can be added here to cover other branches, especially the LogWarning on line 75
        // and the version comparison logic.
    }

    // Helper class to override private methods
    public class TestAbpIoSourceCodeStore : AbpIoSourceCodeStore
    {
        public Func<string, string, string, bool, Task<string>> GetLatestSourceCodeVersionAsyncFunc { get; set; }

        public TestAbpIoSourceCodeStore(
            IOptions<AbpCliOptions> options,
            IJsonSerializer jsonSerializer,
            IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
            ICancellationTokenProvider cancellationTokenProvider,
            CliHttpClientFactory cliHttpClientFactory,
            CliVersionService cliVersionService)
            : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
        {
        }

        protected override Task<string> GetLatestSourceCodeVersionAsync(string name, string type, string s, bool includePreReleases)
        {
            return GetLatestSourceCodeVersionAsyncFunc != null
                ? GetLatestSourceCodeVersionAsyncFunc(name, type, s, includePreReleases)
                : base.GetLatestSourceCodeVersionAsync(name, type, s, includePreReleases);
        }
    }
}
