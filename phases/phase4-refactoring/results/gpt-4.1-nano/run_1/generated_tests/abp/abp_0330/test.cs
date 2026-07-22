using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests
{
    // Minimal stub classes for dependencies
    public class StubOptions : IOptions<AbpCliOptions>
    {
        public AbpCliOptions Value => new AbpCliOptions();
    }

    public class DummyJsonSerializer : IJsonSerializer { }
    public class DummyRemoteServiceExceptionHandler : IRemoteServiceExceptionHandler { }
    public class DummyCancellationTokenProvider : ICancellationTokenProvider { }
    public class DummyCliHttpClientFactory : CliHttpClientFactory { }
    public class DummyCliVersionService : CliVersionService { }

    public class TestableAbpIoSourceCodeStore : AbpIoSourceCodeStore
    {
        private readonly Func<string, bool> _isNetworkSourceFunc;
        private readonly Func<string, byte[]> _readAllBytesFunc;

        public TestableAbpIoSourceCodeStore(
            IOptions<AbpCliOptions> options,
            IJsonSerializer jsonSerializer,
            IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
            ICancellationTokenProvider cancellationTokenProvider,
            CliHttpClientFactory cliHttpClientFactory,
            CliVersionService cliVersionService,
            Func<string, bool> isNetworkSourceFunc,
            Func<string, byte[]> readAllBytesFunc)
            : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
        {
            _isNetworkSourceFunc = isNetworkSourceFunc;
            _readAllBytesFunc = readAllBytesFunc;
        }

        protected override bool IsNetworkSource(string source)
        {
            return _isNetworkSourceFunc(source);
        }

        protected override byte[] ReadAllBytes(string path)
        {
            return _readAllBytesFunc(path);
        }
    }

    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_Should_LogInformation_For_LocalTemplateSource()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new StubOptions();
            var jsonSerializer = new DummyJsonSerializer();
            var remoteHandler = new DummyRemoteServiceExceptionHandler();
            var cancellationTokenProvider = new DummyCancellationTokenProvider();
            var cliHttpFactory = new DummyCliHttpClientFactory();
            var cliVersionService = new DummyCliVersionService();

            var store = new TestableAbpIoSourceCodeStore(
                options,
                jsonSerializer,
                remoteHandler,
                cancellationTokenProvider,
                cliHttpFactory,
                cliVersionService,
                source => false, // IsNetworkSource returns false
                path => new byte[] { 1, 2, 3 } // Dummy file bytes
            );

            store.Logger = loggerMock.Object;

            // Setup parameters
            string name = "TestTemplate";
            string type = "Template";
            string version = "1.0.0";
            string templateSource = "C:\\Templates";

            // Act
            await store.GetAsync(
                name,
                type,
                version,
                templateSource,
                includePreReleases: false,
                skipCache: true,
                trustUserVersion: false);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Using local {type}: {name}, version: {version}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
