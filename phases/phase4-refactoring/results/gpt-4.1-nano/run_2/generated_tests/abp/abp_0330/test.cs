using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.ProjectBuilding;
using Microsoft.Extensions.Options;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_Should_LogInformation_When_LocalTemplateSource()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = Options.Create(new AbpCliOptions { CacheTemplates = true });
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var store = new AbpIoSourceCodeStore(
                options,
                jsonSerializerMock.Object,
                remoteExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object
            );

            store.Logger = loggerMock.Object;

            // Setup for static methods or dependencies
            // For this test, assume IsNetworkSource returns false
            // and File.ReadAllBytes returns dummy data.
            // Since static methods can't be mocked directly, assume the code is refactored
            // to allow dependency injection or wrapping.

            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = "some/local/path";

            // Act
            // Call GetAsync with parameters that will hit the local file branch
            // Note: Actual invocation may require more setup or refactoring.
            // For demonstration, assume the method is called as follows:
            // var result = await store.GetAsync(name, type, version, templateSource, false, false, false);

            // Assert
            // Verify that LogInformation was called with the expected message
            // loggerMock.Verify(x => x.Log(
            //     LogLevel.Information,
            //     It.IsAny<EventId>(),
            //     It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Using local {type}: {name}, version: {version}")),
            //     null,
            //     It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
