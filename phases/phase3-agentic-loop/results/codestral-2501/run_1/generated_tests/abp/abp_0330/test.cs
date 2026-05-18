using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Volo.Abp.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _sourceCodeStore;
        private readonly TestLogger<AbpIoSourceCodeStore> _logger;

        public AbpIoSourceCodeStoreTests()
        {
            _optionsMock = new Mock<IOptions<AbpCliOptions>>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
            _logger = new TestLogger<AbpIoSourceCodeStore>();

            _sourceCodeStore = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object
            )
            {
                Logger = _logger
            };
        }

        [Fact]
        public async Task GetAsync_ShouldLogInformation_WhenUsingLocalTemplate()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = "localSource";

            _cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(new NuGet.Versioning.SemanticVersion(1, 0, 0));
            _cliVersionServiceMock.Setup(x => x.IsNetworkSource(templateSource)).Returns(false);

            // Act
            await _sourceCodeStore.GetAsync(name, type, version, templateSource);

            // Assert
            Assert.Contains(_logger.Logs, log => log.Contains("Using local Template: TestTemplate, version: 1.0.0"));
        }

        [Fact]
        public async Task GetAsync_ShouldLogInformation_WhenUsingCachedTemplate()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = string.Empty;

            _cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(new NuGet.Versioning.SemanticVersion(1, 0, 0));
            _optionsMock.Setup(x => x.Value).Returns(new AbpCliOptions { CacheTemplates = true });

            var localCacheFile = Path.Combine(CliPaths.TemplateCache, name.Replace("/", ".") + "-" + version + ".zip");
            File.WriteAllBytes(localCacheFile, new byte[0]);

            // Act
            await _sourceCodeStore.GetAsync(name, type, version, templateSource);

            // Assert
            Assert.Contains(_logger.Logs, log => log.Contains("Using cached Template: TestTemplate, version: 1.0.0"));

            // Clean up
            File.Delete(localCacheFile);
        }

        [Fact]
        public async Task GetAsync_ShouldLogInformation_WhenDownloadingTemplate()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = string.Empty;

            _cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(new NuGet.Versioning.SemanticVersion(1, 0, 0));
            _optionsMock.Setup(x => x.Value).Returns(new AbpCliOptions { CacheTemplates = false });

            // Act
            await _sourceCodeStore.GetAsync(name, type, version, templateSource);

            // Assert
            Assert.Contains(_logger.Logs, log => log.Contains("Downloading Template: TestTemplate, version: 1.0.0"));
        }
    }

    public class TestLogger<T> : ILogger<T>
    {
        public List<string> Logs { get; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(formatter(state, exception));
        }
    }
}
