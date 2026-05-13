using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_UsesLocalTemplate_WhenConditionsAreMet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions { CacheTemplates = true };
            var jsonSerializer = new Mock<IJsonSerializer>().Object;
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>().Object;
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>().Object;
            var cliHttpClientFactory = new Mock<CliHttpClientFactory>().Object;
            var cliVersionService = new Mock<CliVersionService>().Object;

            var store = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>> { CallBase = true }.Object,
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactory,
                cliVersionService)
            {
                Logger = loggerMock.Object
            };

            string name = "TestTemplate";
            string type = "Template";
            string version = "1.0.0";
            string templateSource = "/local/path";

            // Act
            await store.GetAsync(name, type, version, templateSource);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Using local Template: TestTemplate, version: 1.0.0")),
                    It.IsAny<ILoggerEventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<string, Exception, string>>()),
                Times.Once);
        }
    }
}
