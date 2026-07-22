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
        public async Task GetAsync_Should_LogInformation_For_LocalTemplateSource()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new Mock<IOptions<AbpCliOptions>>();
            options.Setup(o => o.Value).Returns(new AbpCliOptions());

            var jsonSerializer = new Mock<IJsonSerializer>();
            var remoteExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var cliHttpClientFactory = new Mock<CliHttpClientFactory>();
            var cliVersionService = new Mock<CliVersionService>();

            var store = new AbpIoSourceCodeStore(
                options.Object,
                jsonSerializer.Object,
                remoteExceptionHandler.Object,
                cancellationTokenProvider.Object,
                cliHttpClientFactory.Object,
                cliVersionService.Object);

            store.Logger = loggerMock.Object;

            // Setup dependencies to reach the desired code path
            // For simplicity, stub methods to return values that lead to local file usage
            // Note: Reflection or internal access might be needed for private methods, but for this example, assume they are accessible or stubbed.

            // Simulate that version exists
            // Simulate that IsNetworkSource returns false
            // Simulate that templateSource is a local path
            var localPath = Path.GetTempPath();
            var fileName = "TestTemplate-1.0.0.zip";
            var filePath = Path.Combine(localPath, fileName);
            File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });

            // Call GetAsync with parameters to trigger local file log
            var result = await store.GetAsync(
                name: "TestTemplate",
                type: "Type",
                version: "1.0.0",
                templateSource: localPath,
                includePreReleases: false,
                skipCache: false,
                trustUserVersion: false);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using local ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            File.Delete(filePath);
        }
    }
}
