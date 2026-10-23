using Xunit;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Volo.Abp.Http;
using Volo.Abp.IO;
using Volo.Abp.Json;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task GetAsync_LocalTemplate_ReturnsTemplateFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                null,
                cliVersionServiceMock.Object);

            var templateName = "my-template";
            var templateType = "template";
            var templateVersion = "1.0.0";
            var templateSource = Path.Combine(Directory.GetCurrentDirectory(), "templates");

            // Act
            var templateFile = await abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                includePreReleases: false,
                skipCache: false,
                trustUserVersion: false);

            // Assert
            Assert.NotNull(templateFile);
            Assert.Equal(templateVersion, templateFile.Version);
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_CachedTemplate_ReturnsTemplateFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                null,
                cliVersionServiceMock.Object);

            var templateName = "my-template";
            var templateType = "template";
            var templateVersion = "1.0.0";
            var templateSource = Path.Combine(Directory.GetCurrentDirectory(), "templates");

            // Act
            var templateFile = await abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                includePreReleases: false,
                skipCache: false,
                trustUserVersion: false);

            // Assert
            Assert.NotNull(templateFile);
            Assert.Equal(templateVersion, templateFile.Version);
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_DownloadedTemplate_ReturnsTemplateFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                null,
                cliVersionServiceMock.Object);

            var templateName = "my-template";
            var templateType = "template";
            var templateVersion = "1.0.0";
            string? templateSource = null;

            // Act
            var templateFile = await abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                includePreReleases: false,
                skipCache: false,
                trustUserVersion: false);

            // Assert
            Assert.NotNull(templateFile);
            Assert.Equal(templateVersion, templateFile.Version);
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
