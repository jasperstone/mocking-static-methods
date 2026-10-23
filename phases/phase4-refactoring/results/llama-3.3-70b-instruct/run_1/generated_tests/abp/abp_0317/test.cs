using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<CliHttpClientFactory>().Object,
                _cliVersionServiceMock.Object);
            _abpIoSourceCodeStore.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_ThrowsCliUsageException_WhenLatestVersionIsNull()
        {
            // Arrange
            _cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync()).ReturnsAsync(new SemanticVersion(1, 0, 0));

            // Act and Assert
            await Assert.ThrowsAsync<CliUsageException>(() => _abpIoSourceCodeStore.GetAsync("name", "type"));
        }

        [Fact]
        public async Task GetAsync_LogsWarning_WhenLatestVersionIsNull()
        {
            // Arrange
            _cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync()).ReturnsAsync(new SemanticVersion(1, 0, 0));

            // Act
            try
            {
                await _abpIoSourceCodeStore.GetAsync("name", "type");
            }
            catch (CliUsageException)
            {
            }

            // Assert
            _loggerMock.Verify(l => l.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        }
    }
}
