using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.Http;
using Volo.Abp.Threading;
using Microsoft.Extensions.Logging.Abstractions;

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
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<ICliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<ICliVersionService>();

            var store = new AbpIoSourceCodeStore(
                options,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = "local/path";

            // Act
            await store.GetAsync(name, type, version, templateSource);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Using local Template: TestTemplate, version: 1.0.0")),
                    It.IsAny<NullLoggerEventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<string, Exception, string>>()),
                Times.Once);
        }
    }
}
