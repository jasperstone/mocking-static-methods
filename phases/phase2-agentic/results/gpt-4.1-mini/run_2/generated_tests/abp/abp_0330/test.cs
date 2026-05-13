using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly AbpIoSourceCodeStore _store;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();

            var optionsMock = new Moq.Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            var jsonSerializerMock = new Moq.Mock<Volo.Abp.Json.IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Moq.Mock<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Moq.Mock<Volo.Abp.Threading.ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Moq.Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var cliVersionServiceMock = new Moq.Mock<CliVersionService>();

            // Setup CliVersionService to return a fixed version
            cliVersionServiceMock.Setup(c => c.GetCurrentCliVersionAsync())
                .ReturnsAsync(SemanticVersion.Parse("1.0.0"));

            _store = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object
            );

            _store.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_LogsInformation_WhenUsingLocalTemplateSource()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "templateType";
            var version = "1.0.0";
            var templateSource = Path.GetTempPath();

            // Create a dummy zip file to simulate the template file
            var filePath = Path.Combine(templateSource, $"{name}-{version}.zip");
            File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });

            // We need to mock IsNetworkSource to return false for this test
            var storeMock = new Mock<AbpIoSourceCodeStore>(
                new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(new AbpCliOptions()),
                new Moq.Mock<Volo.Abp.Json.IJsonSerializer>().Object,
                new Moq.Mock<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>().Object,
                new Moq.Mock<Volo.Abp.Threading.ICancellationTokenProvider>().Object,
                new Moq.Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>().Object,
                new Moq.Mock<CliVersionService>().Object
            ) { CallBase = true };

            storeMock.Object.Logger = _loggerMock.Object;

            // Setup dependencies for the mock
            storeMock.Setup(s => s.IsNetworkSource(It.IsAny<string>())).Returns(false);
            storeMock.Setup(s => s.IsVersionExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            storeMock.Setup(s => s.GetTemplateNugetVersionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string)null);
            storeMock.Setup(s => s.GetLatestSourceCodeVersionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(version);

            // Act
            var templateFile = await storeMock.Object.GetAsync(name, type, version, templateSource);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Using local {type}: {name}, version: {version}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(templateFile);
            Assert.Equal(version, templateFile.Version);

            // Cleanup
            File.Delete(filePath);
        }
    }
}
