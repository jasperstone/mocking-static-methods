using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_UsesLocalTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = Options.Create(new AbpCliOptions());
            var jsonSerializer = new Mock<IJsonSerializer>().Object;
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>().Object;
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>().Object;
            var cliHttpClientFactory = new Mock<ICliHttpClientFactory>().Object;
            var cliVersionService = new Mock<ICliVersionService>().Object;

            var store = new AbpIoSourceCodeStore(
                options,
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactory,
                cliVersionService)
            {
                Logger = loggerMock.Object
            };

            var templateSource = "local/path";
            var name = "templateName";
            var version = "1.0.0";

            // Act
            await store.GetAsync(name, "Template", version, templateSource);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Using local Template: templateName, version: 1.0.0")),
                    It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<System.Collections.Generic.IDictionary<string, object>>()),
                Times.Once);
        }
    }
}
