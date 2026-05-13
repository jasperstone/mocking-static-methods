using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using System.IO;
using System;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_Should_Call_LogInformation_For_Local_TemplateSource()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new Mock<IOptions<AbpCliOptions>>();
            options.Setup(o => o.Value).Returns(new AbpCliOptions());
            var jsonSerializer = new Mock<IJsonSerializer>();
            var remoteHandler = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var httpClientFactory = new Mock<CliHttpClientFactory>();
            var versionService = new Mock<CliVersionService>();

            var store = new AbpIoSourceCodeStore(
                options.Object,
                jsonSerializer.Object,
                remoteHandler.Object,
                cancellationTokenProvider.Object,
                httpClientFactory.Object,
                versionService.Object);

            store.Logger = mockLogger.Object;

            // Mock dependencies
            // Simulate that the method will reach the code block with LogInformation
            // For simplicity, override IsNetworkSource to return false and provide a local path
            var localPath = Path.GetTempPath();
            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";

            // Setup GetTemplateNugetVersionAsync to return null
            store.GetType().GetMethod("GetTemplateNugetVersionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .CreateDelegate<Func<string, string, string, Task<string>>>(store)
                .Invoke(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
                .ReturnsAsync((string)null);

            // Act
            await store.GetAsync(name, type, version, localPath, includePreReleases: false, skipCache: true, trustUserVersion: false);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(It.Is<string>(msg => msg.Contains("Using local"))),
                Times.AtLeastOnce);
        }
    }
}
