using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_UsesLocalTemplate_WhenTemplateSourceIsLocal()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions();
            var jsonSerializer = new Mock<IJsonSerializer>().Object;
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>().Object;
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>().Object;
            var cliHttpClientFactory = new Mock<CliHttpClientFactory>().Object;
            var cliVersionService = new Mock<CliVersionService>().Object;

            var store = new AbpIoSourceCodeStore(
                options,
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactory,
                cliVersionService,
                loggerMock.Object) // Inject the mock logger
            {
                Logger = loggerMock.Object
            };

            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = "/local/path";

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
