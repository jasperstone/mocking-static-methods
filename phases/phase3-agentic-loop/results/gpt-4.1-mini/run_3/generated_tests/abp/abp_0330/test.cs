using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    private class TestCliVersionService : CliVersionService
    {
        private readonly NuGetVersion _version;

        public TestCliVersionService(NuGetVersion version) : base(null, null, null)
        {
            _version = version;
        }

        public new Task<NuGetVersion> GetCurrentCliVersionAsync()
        {
            return Task.FromResult(_version);
        }
    }

    [Fact]
    public async Task GetAsync_LogsInformation_WhenUsingLocalTemplateSource()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions { CacheTemplates = false });
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

        // Use a real CliVersionService subclass to avoid Moq proxy issues
        var cliVersionService = new TestCliVersionService(NuGetVersion.Parse("1.0.0"));

        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProviderMock.Object,
            null, // Pass null for CliHttpClientFactory as it is not used in this test path
            cliVersionService)
        {
            Logger = loggerMock.Object
        };

        var name = "TestTemplate";
        var type = SourceCodeTypes.Template;
        var version = "1.0.0";
        var templateSource = Path.GetTempPath();
        var filePath = Path.Combine(templateSource, $"{name}-{version}.zip");
        var fileContent = new byte[] { 1, 2, 3 };

        // Create a dummy file to simulate local template source
        File.WriteAllBytes(filePath, fileContent);

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
        Assert.Equal(fileContent, templateFile.FileBytes);

        // Cleanup
        File.Delete(filePath);
    }
}
