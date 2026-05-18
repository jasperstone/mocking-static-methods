using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_RemoteServiceUnavailable_LogsWarningAndThrowsException()
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
                cliVersionServiceMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(new NuGet.Versioning.SemanticVersion(1, 0, 0));
            cliVersionServiceMock.Setup(x => x.GetLatestSourceCodeVersionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync((NuGet.Versioning.SemanticVersion)null);

            // Act
            var exception = await Record.ExceptionAsync(() => store.GetAsync("templateName", "templateType"));

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("The remote service is currently unavailable, please specify the version."),
                Times.Once
            );

            Assert.IsType<CliUsageException>(exception);
            Assert.Equal("Use command: abp new Acme.BookStore -v version", exception.Message);
        }
    }
}
