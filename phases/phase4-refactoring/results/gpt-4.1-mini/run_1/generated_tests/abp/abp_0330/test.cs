using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    private class FakeCancellationTokenProvider : ICancellationTokenProvider
    {
        public System.Threading.CancellationToken Token => System.Threading.CancellationToken.None;
    }

    [Fact]
    public async Task GetAsync_LogsInformation_WhenUsingLocalTemplateSource()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new AbpCliOptions { CacheTemplates = false });
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cancellationTokenProvider = new FakeCancellationTokenProvider();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var cliVersionServiceMock = new Mock<CliVersionService>(cliHttpClientFactoryMock.Object, null);

        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();

        var store = new AbpIoSourceCodeStore(
            options,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProvider,
            cliHttpClientFactoryMock.Object,
            cliVersionServiceMock.Object);

        store.Logger = loggerMock.Object;

        var name = "TestTemplate";
        var type = SourceCodeTypes.Template;
        var version = "1.0.0";
        var templateSource = Path.GetTempPath();

        var filePath = Path.Combine(templateSource, $"{name}-{version}.zip");
        File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });

        // Use reflection to set private methods to return expected values
        var isVersionExistsMethod = typeof(AbpIoSourceCodeStore).GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var getTemplateNugetVersionAsyncMethod = typeof(AbpIoSourceCodeStore).GetMethod("GetTemplateNugetVersionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Setup IsVersionExists to return true
        var isVersionExistsTask = (Task<bool>)isVersionExistsMethod.Invoke(store, new object[] { name, version });
        await isVersionExistsTask;

        // Setup GetTemplateNugetVersionAsync to return null
        var getTemplateNugetVersionTask = (Task<string>)getTemplateNugetVersionAsyncMethod.Invoke(store, new object[] { name, type, version });
        await getTemplateNugetVersionTask;

        // Act
        var templateFile = await store.GetAsync(name, type, version, templateSource);

        // Assert
        loggerMock.Verify(l => l.Log(
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
