using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _store;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            _cliVersionServiceMock = new Mock<CliVersionService>();
            _store = new AbpIoSourceCodeStore(
                optionsMock.Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<CliHttpClientFactory>().Object,
                _cliVersionServiceMock.Object
            );
            _store.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_Should_LogWarning_When_LatestVersionIsNull()
        {
            // Arrange
            var store = _store;
            var name = "TestTemplate";
            var type = "Template";
            var version = (string)null;
            var includePreReleases = false;
            var skipCache = false;
            var trustUserVersion = false;

            // Mock GetLatestSourceCodeVersionAsync to return null
            var storeMock = new Mock<AbpIoSourceCodeStore>(
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<CliHttpClientFactory>().Object,
                new Mock<CliVersionService>().Object
            );
            storeMock.Setup(s => s.GetLatestSourceCodeVersionAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<bool>()))
                .ReturnsAsync((string)null);
            storeMock.Object.Logger = _loggerMock.Object;

            // Act
            await Assert.ThrowsAsync<CliUsageException>(async () =>
                await storeMock.Object.GetAsync(name, type, version, null, includePreReleases, skipCache, trustUserVersion));

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        }
    }
}
