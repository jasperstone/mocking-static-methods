using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_ValidInput_ReturnsTemplateFile()
        {
            // Arrange
            var logger = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var jsonSerializer = new Mock<Volo.Abp.Json.IJsonSerializer>();
            var remoteServiceExceptionHandler = new Mock<Volo.Abp.Cli.ProjectBuilding.IRemoteServiceExceptionHandler>();
            var cancellationTokenProvider = new Mock<Volo.Abp.Threading.ICancellationTokenProvider>();
            var cliVersionService = new Mock<Volo.Abp.Cli.Version.CliVersionService>();
            var cliHttpClientFactory = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<Microsoft.Extensions.Options.IOptions<Volo.Abp.Cli.AbpCliOptions>>().Object,
                jsonSerializer.Object,
                remoteServiceExceptionHandler.Object,
                cancellationTokenProvider.Object,
                cliHttpClientFactory.Object,
                cliVersionService.Object);

            // Act
            var result = await abpIoSourceCodeStore.GetAsync("template-name", "template-type", "version");

            // Assert
            Assert.NotNull(result);
        }
    }
}
