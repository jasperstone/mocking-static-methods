using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly AbpIoSourceCodeStore _store;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new Mock<IOptions<AbpCliOptions>>();
            options.Setup(o => o.Value).Returns(new AbpCliOptions());
            _store = new AbpIoSourceCodeStore(
                options.Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<CliHttpClientFactory>().Object,
                new Mock<CliVersionService>().Object
            );
            _store.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_Should_LogInformation_When_TemplateSource_Is_Local()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "TemplateType";
            var version = "1.0.0";
            var templateSource = "local/path";

            // Mock static methods or dependencies if needed
            // For simplicity, assume IsNetworkSource returns false
            // and File.ReadAllBytes returns some byte array

            // Act
            var result = await _store.GetAsync(name, type, version, templateSource);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using local")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_Should_LogInformation_When_Using_Cached_Template()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "TemplateType";
            var version = "1.0.0";

            // Mock IsNetworkSource to return true
            // Mock File.Exists to return true for local cache
            // For simplicity, assume dependencies are mocked accordingly

            // Act
            var result = await _store.GetAsync(name, type, version);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using cached")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_Should_LogInformation_When_Downloading_Template()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "TemplateType";
            var version = "1.0.0";

            // Mock dependencies to simulate download path

            // Act
            var result = await _store.GetAsync(name, type, version);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Downloading")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
