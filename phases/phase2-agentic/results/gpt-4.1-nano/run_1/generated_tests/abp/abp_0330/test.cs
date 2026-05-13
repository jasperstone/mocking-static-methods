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
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly AbpIoSourceCodeStore _store;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var options = new Mock<IOptions<AbpCliOptions>>();
            options.Setup(o => o.Value).Returns(new AbpCliOptions());
            _store = new AbpIoSourceCodeStore(
                options.Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<CliHttpClientFactory>().Object,
                new Mock<CliVersionService>().Object
            );
            _store.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_Should_LogInformation_When_TemplateSource_Is_Local()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "Template";
            var version = "1.0.0";
            var templateSource = "local/path";
            var mockTemplateBytes = new byte[] { 1, 2, 3 };
            var templateFilePath = Path.Combine(templateSource, name + "-" + version + ".zip");
            Directory.CreateDirectory(templateSource);
            File.WriteAllBytes(templateFilePath, mockTemplateBytes);

            // Act
            var result = await _store.GetAsync(name, type, version, templateSource, includePreReleases: false, skipCache: true, trustUserVersion: false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using local ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.NotNull(result);
            Assert.Equal(templateFilePath, Path.Combine(templateSource, name + "-" + version + ".zip"));
            Directory.Delete(templateSource, true);
        }
    }
}
