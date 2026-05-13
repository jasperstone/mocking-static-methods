using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.Http;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _optionsMock = new Mock<IOptions<AbpCliOptions>>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(null, null);
            _cliVersionServiceMock = new Mock<CliVersionService>(null, null);

            _optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions
            {
                CacheTemplates = false
            });
        }

        [Fact]
        public async Task GetAsync_LogsInformation_WhenUsingLocalTemplateSource()
        {
            // Arrange
            var name = "TestTemplate";
            var type = "templateType";
            var version = "1.0.0";
            var templateSource = Path.GetTempPath(); // Use temp path as local source
            var filePath = Path.Combine(templateSource, $"{name}-{version}.zip");

            // Create a dummy file to simulate the template zip
            File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });

            var store = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };

            // Setup mocks for methods called inside GetAsync
            _cliVersionServiceMock.Setup(c => c.GetCurrentCliVersionAsync())
                .ReturnsAsync(new NuGet.Versioning.SemanticVersion(1, 0, 0));
            // We need to mock IsVersionExists and GetTemplateNugetVersionAsync to avoid exceptions
            var privateObject = new PrivateObject(store);
            privateObject.SetFieldOrProperty("IsVersionExists", new Func<string, string, Task<bool>>((n, v) => Task.FromResult(true)));
            privateObject.SetFieldOrProperty("GetTemplateNugetVersionAsync", new Func<string, string, string, Task<string>>((n, t, v) => Task.FromResult(v)));

            // Act
            var templateFile = await store.GetAsync(name, type, version, templateSource);

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
