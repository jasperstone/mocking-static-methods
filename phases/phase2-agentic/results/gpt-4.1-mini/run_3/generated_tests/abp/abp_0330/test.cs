using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task GetAsync_LogsInformation_WhenUsingLocalTemplateSource()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions { CacheTemplates = false });
        var jsonSerializerMock = new Mock<Volo.Abp.Json.IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.Http.IRemoteServiceExceptionHandler>();
        var cancellationTokenProviderMock = new Mock<Volo.Abp.Threading.ICancellationTokenProvider>();
        var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProviderMock.Object,
            cliHttpClientFactoryMock.Object,
            cliVersionServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        var name = "TestTemplate";
        var type = "templateType";
        var version = "1.0.0";
        var templateSource = Path.GetTempPath(); // Use temp path as local source

        // Create a dummy zip file to simulate the template file
        var filePath = Path.Combine(templateSource, $"{name}-{version}.zip");
        File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });

        // Setup mocks for version checks
        cliVersionServiceMock.Setup(c => c.GetCurrentCliVersionAsync())
            .ReturnsAsync(new NuGet.Versioning.SemanticVersion(1, 0, 0));
        // We override IsVersionExists and GetTemplateNugetVersionAsync by reflection or by subclassing
        // but since they are private, we will use a derived class to override them for testing

        var testStore = new TestAbpIoSourceCodeStore(store, filePath);

        // Act
        var templateFile = await testStore.GetAsync(name, type, version, templateSource);

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
        private readonly string _filePath;

        public TestAbpIoSourceCodeStore(AbpIoSourceCodeStore baseStore, string filePath)
            : base(
                new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(new AbpCliOptions()),
                baseStore.JsonSerializer,
                baseStore.RemoteServiceExceptionHandler,
                baseStore.CancellationTokenProvider,
                new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>().Object,
                baseStore.CliVersionService)
        {
            Logger = baseStore.Logger;
            _filePath = filePath;
        }

        protected override Task<bool> IsVersionExists(string name, string version)
        {
            return Task.FromResult(true);
        }

        protected override Task<string> GetTemplateNugetVersionAsync(string name, string type, string version)
        {
            return Task.FromResult<string>(null);
        }

        protected override bool IsNetworkSource(string source)
        {
            return false;
        }

        protected override Task<string> GetLatestSourceCodeVersionAsync(string name, string type, string preRelease)
        {
            return Task.FromResult("1.0.0");
        }

        protected override Task<NuGet.Versioning.SemanticVersion> GetCurrentCliVersionAsync()
        {
            return Task.FromResult(new NuGet.Versioning.SemanticVersion(1, 0, 0));
        }

        protected override Task<NuGet.Versioning.SemanticVersion> ParseSemanticVersionAsync(string version)
        {
            return Task.FromResult(NuGet.Versioning.SemanticVersion.Parse(version));
        }

        protected override Task<TemplateFile> GetTemplateFileAsync(string path, string version, string latestVersion, string nugetVersion)
        {
            var bytes = File.ReadAllBytes(_filePath);
            return Task.FromResult(new TemplateFile(bytes, version, latestVersion, nugetVersion));
        }
    }
}
