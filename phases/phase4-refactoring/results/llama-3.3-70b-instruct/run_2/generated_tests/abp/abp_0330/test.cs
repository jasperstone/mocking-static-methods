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
        public async Task GetAsync_UsesLocalTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions { CacheTemplates = true };
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>().SetupGet(o => o.Value).Returns(options).Object,
                new Mock<Volo.Abp.Json.IJsonSerializer>().Object,
                new Mock<Volo.Abp.Cli.IRemoteServiceExceptionHandler>().Object,
                new Mock<Volo.Abp.Threading.ICancellationTokenProvider>().Object,
                new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>().Object,
                new Mock<Volo.Abp.Cli.Version.CliVersionService>().Object);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateSource = Path.Combine(Directory.GetCurrentDirectory(), "template-source");
            Directory.CreateDirectory(templateSource);

            var templateFile = Path.Combine(templateSource, "template-name-1.0.0.zip");
            File.Create(templateFile).Dispose();

            // Act
            await abpIoSourceCodeStore.GetAsync("template-name", "template-type", "1.0.0", templateSource);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Using local template-type: template-name, version: 1.0.0"))),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_UsesCachedTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions { CacheTemplates = true };
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>().SetupGet(o => o.Value).Returns(options).Object,
                new Mock<Volo.Abp.Json.IJsonSerializer>().Object,
                new Mock<Volo.Abp.Cli.IRemoteServiceExceptionHandler>().Object,
                new Mock<Volo.Abp.Threading.ICancellationTokenProvider>().Object,
                new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>().Object,
                new Mock<Volo.Abp.Cli.Version.CliVersionService>().Object);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            var templateCache = Path.Combine(Directory.GetCurrentDirectory(), "template-cache");
            Directory.CreateDirectory(templateCache);

            var templateFile = Path.Combine(templateCache, "template-name-1.0.0.zip");
            File.Create(templateFile).Dispose();

            // Act
            await abpIoSourceCodeStore.GetAsync("template-name", "template-type", "1.0.0", null, false, false, false);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Using cached template-type: template-name, version: 1.0.0"))),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_DownloadsTemplate_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new AbpCliOptions { CacheTemplates = true };
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>().SetupGet(o => o.Value).Returns(options).Object,
                new Mock<Volo.Abp.Json.IJsonSerializer>().Object,
                new Mock<Volo.Abp.Cli.IRemoteServiceExceptionHandler>().Object,
                new Mock<Volo.Abp.Threading.ICancellationTokenProvider>().Object,
                new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>().Object,
                new Mock<Volo.Abp.Cli.Version.CliVersionService>().Object);

            abpIoSourceCodeStore.Logger = loggerMock.Object;

            // Act
            await abpIoSourceCodeStore.GetAsync("template-name", "template-type", "1.0.0", null, false, true, false);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Downloading template-type: template-name, version: 1.0.0"))),
                Times.Once);
        }
    }
}
