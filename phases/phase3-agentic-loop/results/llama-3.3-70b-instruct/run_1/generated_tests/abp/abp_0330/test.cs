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
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<Microsoft.Extensions.Options.IOptions<Volo.Abp.Cli.Core.AbpCliOptions>>().Object,
                new Mock<Volo.Abp.Json.IJsonSerializer>().Object,
                new Mock<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>().Object,
                new Mock<Volo.Abp.Threading.ICancellationTokenProvider>().Object,
                new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>().Object,
                new Mock<Volo.Abp.Cli.Version.CliVersionService>().Object
            );
            _abpIoSourceCodeStore.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_UsesLocalTemplate_LogsInformation()
        {
            // Arrange
            var templateName = "template-name";
            var templateType = "template-type";
            var templateVersion = "template-version";
            var templateSource = "template-source";

            // Act
            await _abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                false,
                false,
                false
            );

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Using local {templateType}: {templateName}, version: {templateVersion}"))
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAsync_UsesCachedTemplate_LogsInformation()
        {
            // Arrange
            var templateName = "template-name";
            var templateType = "template-type";
            var templateVersion = "template-version";
            var templateSource = string.Empty;

            // Act
            await _abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                false,
                false,
                false
            );

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Using cached {templateType}: {templateName}, version: {templateVersion}"))
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAsync_DownloadsTemplate_LogsInformation()
        {
            // Arrange
            var templateName = "template-name";
            var templateType = "template-type";
            var templateVersion = "template-version";
            var templateSource = string.Empty;

            // Act
            await _abpIoSourceCodeStore.GetAsync(
                templateName,
                templateType,
                templateVersion,
                templateSource,
                false,
                true,
                false
            );

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message.Contains($"Downloading {templateType}: {templateName}, version: {templateVersion}"))
                ),
                Times.Once
            );
        }
    }
}
