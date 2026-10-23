using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Microsoft.Extensions.Options;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task GetAsync_LogsInformation_WhenUsingLocalTemplateSource()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions { CacheTemplates = false });
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
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
        var type = SourceCodeTypes.Template;
        var version = "1.0.0";
        var templateSource = Path.GetTempPath();

        // Create a dummy zip file to simulate local template source
        var filePath = Path.Combine(templateSource, $"{name}-{version}.zip");
        File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });

        // Setup mocks for version checks
        cliVersionServiceMock.Setup(c => c.GetCurrentCliVersionAsync())
            .ReturnsAsync(new SemanticVersion(1, 0, 0));

        // Setup minimal behavior for private methods via reflection or just rely on actual implementation
        // We expect IsVersionExists to return true, so no exception is thrown
        // We expect GetTemplateNugetVersionAsync to return version, so no null

        // Act
        var templateFile = await store.GetAsync(name, type, version, templateSource);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using local " + type + ": " + name + ", version: " + version)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.NotNull(templateFile);
        Assert.Equal(version, templateFile.Version);

        // Cleanup
        File.Delete(filePath);
    }
}
