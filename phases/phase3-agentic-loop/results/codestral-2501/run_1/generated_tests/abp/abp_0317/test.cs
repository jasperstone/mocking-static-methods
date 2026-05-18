using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Threading;
using Volo.Abp.Json;
using Volo.Abp.Http;
using Volo.Abp.IO;
using System.Threading.Tasks;
using System;
using System.Net.Http;

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
            );

            store.Logger = loggerMock.Object;

            // Mock the GetLatestSourceCodeVersionAsync method to return null
            var getLatestSourceCodeVersionAsyncMock = new Mock<Func<string, string, string, bool, Task<string>>>();
            getLatestSourceCodeVersionAsyncMock.Setup(x => x.Invoke("templateName", "templateType", null, false)).ReturnsAsync((string)null);

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
