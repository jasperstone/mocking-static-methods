using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    private class FakeCliVersionService : CliVersionService
    {
        public FakeCliVersionService() : base(null, null) { }

        public override Task<SemanticVersion> GetCurrentCliVersionAsync()
        {
            return Task.FromResult(new SemanticVersion(1, 0, 0));
        }
    }

    [Fact]
    public async Task GetAsync_LogsInformation_WhenUsingLocalTemplateSource()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>();
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

        var cliHttpClientFactory = new CliHttpClientFactory(null, null);
        var cliVersionService = new FakeCliVersionService();

        var abpCliOptions = new AbpCliOptions();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProviderMock.Object,
            cliHttpClientFactory,
            cliVersionService);

        store.Logger = loggerMock.Object;

        // Setup parameters
        var name = "TestTemplate";
        var type = SourceCodeTypes.Template;
        var version = "1.0.0";
        var templateSource = Path.GetTempPath(); // Use temp path as local source

        // Create a dummy zip file to simulate the template file
        var filePath = Path.Combine(templateSource, $"{name}-{version}.zip");
        File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });

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
}
