using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;

namespace Volo.Abp.Cli.Tests
{
    // Minimal interfaces to replace missing dependencies
    public interface IJsonSerializer { }
    public interface ICancellationTokenProvider { }
    public class CliHttpClientFactory { }

    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
        }

        private class TestAbpIoSourceCodeStore : AbpIoSourceCodeStore
        {
            private readonly bool _isNetworkSource;

            public TestAbpIoSourceCodeStore(
                IOptions<AbpCliOptions> options,
                IJsonSerializer jsonSerializer,
                IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
                ICancellationTokenProvider cancellationTokenProvider,
                CliHttpClientFactory cliHttpClientFactory,
                CliVersionService cliVersionService,
                bool isNetworkSource)
                : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
            {
                _isNetworkSource = isNetworkSource;
            }

            protected override bool IsNetworkSource(string source)
            {
                return _isNetworkSource;
            }
        }

        [Fact]
        public async Task GetAsync_Should_LogInformation_For_LocalTemplate()
        {
            // Arrange
            var options = Options.Create(new AbpCliOptions());
            var store = new TestAbpIoSourceCodeStore(
                options,
                Mock.Of<IJsonSerializer>(),
                Mock.Of<IRemoteServiceExceptionHandler>(),
                Mock.Of<ICancellationTokenProvider>(),
                new CliHttpClientFactory(),
                _cliVersionServiceMock.Object,
                false // simulate local source
            );
            store.Logger = _loggerMock.Object;

            var name = "TestTemplate";
            var type = "TemplateType";
            var version = "1.0.0";
            var templateSource = "some/local/path";

            // Mock GetTemplateNugetVersionAsync to return null
            var mockGetNugetVersion = new Mock<AbpIoSourceCodeStore>(
                Options.Create(new AbpCliOptions()),
                Mock.Of<IJsonSerializer>(),
                Mock.Of<IRemoteServiceExceptionHandler>(),
                Mock.Of<ICancellationTokenProvider>(),
                new CliHttpClientFactory(),
                _cliVersionServiceMock.Object)
            { CallBase = true };
            mockGetNugetVersion.Setup(s => s.GetTemplateNugetVersionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string)null);

            // Mock GetLatestSourceCodeVersionAsync to return a version
            var mockGetLatestVersion = new Mock<AbpIoSourceCodeStore>(
                Options.Create(new AbpCliOptions()),
                Mock.Of<IJsonSerializer>(),
                Mock.Of<IRemoteServiceExceptionHandler>(),
                Mock.Of<ICancellationTokenProvider>(),
                new CliHttpClientFactory(),
                _cliVersionServiceMock.Object)
            { CallBase = true };
            mockGetLatestVersion.Setup(s => s.GetLatestSourceCodeVersionAsync(It.IsAny<string>(), It.IsAny<string>(), null, false)).ReturnsAsync("1.0.0");

            // Mock GetLocalTemplates to return empty list
            var mockGetLocalTemplates = new Mock<AbpIoSourceCodeStore>(
                Options.Create(new AbpCliOptions()),
                Mock.Of<IJsonSerializer>(),
                Mock.Of<IRemoteServiceExceptionHandler>(),
                Mock.Of<ICancellationTokenProvider>(),
                new CliHttpClientFactory(),
                _cliVersionServiceMock.Object)
            { CallBase = true };
            mockGetLocalTemplates.Setup(s => s.GetLocalTemplates()).Returns(Array.Empty<TemplateFile>());

            // Act
            await mockGetLocalTemplates.Object.GetAsync(
                name,
                type,
                version,
                templateSource,
                includePreReleases: false,
                skipCache: false,
                trustUserVersion: false
            );

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using local")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
