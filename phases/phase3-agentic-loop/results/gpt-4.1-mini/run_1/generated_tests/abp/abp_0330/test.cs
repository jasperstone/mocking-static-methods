using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task GetAsync_LogsInformation_WhenUsingLocalSource()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        // Instead of mocking CliHttpClientFactory, pass null because it is not used in this test path
        CliHttpClientFactory cliHttpClientFactory = null;
        var cliVersionServiceMock = new Mock<CliVersionService>(null, null);

        var abpCliOptions = new AbpCliOptions();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var store = new TestAbpIoSourceCodeStore(
            optionsMock.Object,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProviderMock.Object,
            cliHttpClientFactory,
            cliVersionServiceMock.Object,
            loggerMock.Object);

        var name = "TestTemplate";
        var type = "templateType";
        var version = "1.0.0";
        var templateSource = Path.GetTempPath();

        // Create a dummy zip file to simulate the template file
        var filePath = Path.Combine(templateSource, $"{name}-{version}.zip");
        File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });

        cliVersionServiceMock.Setup(c => c.GetCurrentCliVersionAsync())
            .ReturnsAsync(new SemanticVersion(1, 0, 0));

        // Act
        var templateFile = await store.GetAsync(name, type, version, templateSource);

        // Assert
        loggerMock.Verify(
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

    private class TestAbpIoSourceCodeStore : AbpIoSourceCodeStore
    {
        private readonly ILogger<AbpIoSourceCodeStore> _logger;

        public TestAbpIoSourceCodeStore(
            IOptions<AbpCliOptions> options,
            IJsonSerializer jsonSerializer,
            IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
            ICancellationTokenProvider cancellationTokenProvider,
            CliHttpClientFactory cliHttpClientFactory,
            CliVersionService cliVersionService,
            ILogger<AbpIoSourceCodeStore> logger)
            : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
        {
            _logger = logger;
            Logger = logger;
        }

        // Hide IsNetworkSource to treat source as local
        public new bool IsNetworkSource(string source)
        {
            return false;
        }

        // Provide dummy implementations for private methods by new methods (not override)
        public new Task<bool> IsVersionExists(string name, string version)
        {
            return Task.FromResult(true);
        }

        public new Task<string> GetTemplateNugetVersionAsync(string name, string type, string version)
        {
            return Task.FromResult<string>(null);
        }
    }
}
