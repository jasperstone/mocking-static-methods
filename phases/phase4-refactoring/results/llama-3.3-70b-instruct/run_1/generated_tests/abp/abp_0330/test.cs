using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_LogInformationCalled_WhenUsingLocalTemplate()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions();
            var jsonSerializer = new Volo.Abp.Json.SystemTextJson.SystemTextJsonSerializer();
            var remoteServiceExceptionHandler = new Volo.Abp.Cli.ProjectBuilding.RemoteServiceExceptionHandler(jsonSerializer);
            var cancellationTokenProvider = new Volo.Abp.Threading.DefaultCancellationTokenProvider();
            var cliHttpClientFactory = new Volo.Abp.Cli.Http.CliHttpClientFactory(new Volo.Abp.Http.DefaultHttpClientFactory(), cancellationTokenProvider);
            var cliVersionService = new Volo.Abp.Cli.CliVersionService();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                Microsoft.Extensions.Options.Options.Create(options),
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactory,
                cliVersionService);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateSource = Path.Combine(Directory.GetCurrentDirectory(), "template-source");
            Directory.CreateDirectory(templateSource);

            var templateFile = Path.Combine(templateSource, "template-file.zip");
            File.Create(templateFile).Dispose();

            // Act
            await abpIoSourceCodeStore.GetAsync("template-name", "template-type", "template-version", templateSource);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
