using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AbpIoSourceCodeStoreTests
{
    // Minimal stub classes for dependencies
    public class AbpCliOptions { }
    public interface IOptions<T> { T Value { get; } }
    public interface ICancellationTokenProvider { }
    public class CliHttpClientFactory { }
    public class CliVersionService
    {
        public virtual Task<SemanticVersion> GetCurrentCliVersionAsync() => Task.FromResult(new SemanticVersion(1, 0, 0));
    }
    public interface IJsonSerializer { }
    public interface IRemoteServiceExceptionHandler { }

    public class GetAsyncTests
    {
        [Fact]
        public async Task GetAsync_Should_LogWarning_When_LatestVersionIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var mockOptions = new Mock<IOptions<AbpCliOptions>>();
            var mockJsonSerializer = new Mock<IJsonSerializer>();
            var mockRemoteExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockCliVersionService = new Mock<CliVersionService>();

            var store = new TestableAbpIoSourceCodeStore(
                mockOptions.Object,
                mockJsonSerializer.Object,
                mockRemoteExceptionHandler.Object,
                mockCancellationTokenProvider.Object,
                mockHttpClientFactory.Object,
                mockCliVersionService.Object);

            // Inject the mock logger
            store.Logger = mockLogger.Object;

            // Setup dependencies
            mockOptions.Setup(o => o.Value).Returns(new AbpCliOptions());
            mockCliVersionService.Setup(s => s.GetCurrentCliVersionAsync()).ReturnsAsync(new SemanticVersion(1, 0, 0));
            // Setup GetLatestSourceCodeVersionAsync to return null
            store.SetupGetLatestSourceCodeVersionAsync(null);

            // Setup GetLocalTemplates to return some templates
            store.SetupGetLocalTemplates(new List<TemplateInfo>
            {
                new TemplateInfo { TemplateName = "TestTemplate", Version = "1.0.0" }
            });

            // Act & Assert
            await Assert.ThrowsAsync<CliUsageException>(async () =>
                await store.GetAsync("name", "type"));

            // Verify that LogWarning was called with the expected message
            mockLogger.Verify(
                logger => logger.LogWarning("The remote service is currently unavailable, please specify the version."),
                Times.Once);
        }

        // Helper classes for testing
        private class TestableAbpIoSourceCodeStore : AbpIoSourceCodeStore
        {
            private string _latestVersionToReturn;
            private List<TemplateInfo> _localTemplates;

            public TestableAbpIoSourceCodeStore(
                IOptions<AbpCliOptions> options,
                IJsonSerializer jsonSerializer,
                IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
                ICancellationTokenProvider cancellationTokenProvider,
                CliHttpClientFactory cliHttpClientFactory,
                CliVersionService cliVersionService)
                : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
            {
            }

            public void SetupGetLatestSourceCodeVersionAsync(string version)
            {
                _latestVersionToReturn = version;
            }

            public void SetupGetLocalTemplates(List<TemplateInfo> templates)
            {
                _localTemplates = templates;
            }

            protected override Task<string> GetLatestSourceCodeVersionAsync(string name, string type, object p3, bool includePreReleases)
            {
                return Task.FromResult(_latestVersionToReturn);
            }

            protected override List<TemplateInfo> GetLocalTemplates()
            {
                return _localTemplates ?? new List<TemplateInfo>();
            }
        }

        private class TemplateInfo
        {
            public string TemplateName { get; set; }
            public string Version { get; set; }
        }
    }
}
