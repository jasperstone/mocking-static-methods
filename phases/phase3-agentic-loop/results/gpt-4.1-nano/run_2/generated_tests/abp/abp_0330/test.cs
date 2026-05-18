using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.IO;
using System.IO;
using NuGet.Versioning;

namespace Test.Abp.Cli.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_Should_LogInformation_When_TemplateSource_Is_Local()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = Options.Create(new AbpCliOptions());
            var jsonSerializer = new Mock<IJsonSerializer>().Object;
            var remoteHandler = new Mock<IRemoteServiceExceptionHandler>().Object;
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>().Object;
            var httpClientFactory = new Mock<CliHttpClientFactory>().Object;
            var cliVersionServiceMock = new Mock<CliVersionService>();
            cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync()).ReturnsAsync(NuGetVersion.Parse("1.0.0"));

            var store = new AbpIoSourceCodeStore(
                options,
                jsonSerializer,
                remoteHandler,
                cancellationTokenProvider,
                httpClientFactory,
                cliVersionServiceMock.Object);

            store.Logger = loggerMock.Object;

            // Setup a dummy template source that is local
            string templateSource = Path.GetTempPath(); // temp directory
            string name = "TestTemplate";
            string type = "Template";
            string version = "1.0.0";

            // Create dummy file to simulate existing cache
            var cacheFileName = name.Replace("/", ".") + "-" + version + ".zip";
            var localCacheFilePath = Path.Combine(CliPaths.TemplateCache, cacheFileName);
            DirectoryHelper.CreateIfNotExists(CliPaths.TemplateCache);
            File.WriteAllBytes(localCacheFilePath, new byte[] { 1, 2, 3 });

            // Act
            await store.GetAsync(
                name,
                type,
                version,
                templateSource,
                includePreReleases: false,
                skipCache: false,
                trustUserVersion: true);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using local " + type)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
