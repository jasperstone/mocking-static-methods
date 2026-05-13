using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_LogsWarning_WhenRemoteServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var store = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Mock the GetLatestSourceCodeVersionAsync method to return null
            store.GetType().GetMethod("GetLatestSourceCodeVersionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .ReturnsAsync((string name, string type, string version, bool includePreReleases) => null);

            // Act
            await Assert.ThrowsAsync<CliUsageException>(() => store.GetAsync("templateName", "templateType"));

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("The remote service is currently unavailable, please specify the version."),
                Times.Once);
        }
    }
}
