using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests : IClassFixture<AbpIoSourceCodeStoreTestFixture>
{
    private readonly AbpIoSourceCodeStoreTestFixture _fixture;

    public AbpIoSourceCodeStoreTests(AbpIoSourceCodeStoreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_LogInformation_When_Using_Local_NonNetwork_TemplateSource()
    {
        // Arrange
        var templateSource = Path.Combine(Path.GetTempPath(), "local-template-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(templateSource);
        var name = "test-template";
        var version = "1.0.0";
        var type = SourceCodeTypes.Template;
        var expectedMessage = "Using local " + type + ": " + name + ", version: " + version;

        try
        {
            var zipPath = Path.Combine(templateSource, $"{name}-{version}.zip");
            await File.WriteAllBytesAsync(zipPath, new byte[0]);

            // Setup mocks to avoid exceptions before reaching the local source check
            _fixture.CliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync())
                .ReturnsAsync(new Volo.Abp.Cli.Version.SemanticVersion(1, 0, 0));
            
            _fixture.JsonSerializerMock.Setup(x => x.IsValidVersionNumber(It.IsAny<string>()))
                .Returns(true);

            var store = _fixture.ServiceProvider.GetRequiredService<AbpIoSourceCodeStore>();

            // Act
            await store.GetAsync(name, type, version, templateSource);

            // Assert - verify the specific LogInformation call from line 170
            _fixture.LoggerMock.Verify(
                x => x.LogInformation(expectedMessage),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(templateSource))
            {
                Directory.Delete(templateSource, true);
            }
        }
    }
}

public class AbpIoSourceCodeStoreTestFixture : IDisposable
{
    public ServiceProvider ServiceProvider { get; }
    public Mock<ILogger<AbpIoSourceCodeStore>> LoggerMock { get; }
    public Mock<IJsonSerializer> JsonSerializerMock { get; }
    public Mock<IRemoteServiceExceptionHandler> RemoteServiceExceptionHandlerMock { get; }
    public Mock<Volo.Abp.Cli.Version.CliVersionService> CliVersionServiceMock { get; }

    public AbpIoSourceCodeStoreTestFixture()
    {
        LoggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        
        JsonSerializerMock = new Mock<IJsonSerializer>();
        RemoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        CliVersionServiceMock = new Mock<Volo.Abp.Cli.Version.CliVersionService>();

        var services = new ServiceCollection();
        
        services.AddSingleton(Options.Create(new AbpCliOptions()));
        services.AddSingleton(LoggerMock.Object);
        services.AddSingleton(JsonSerializerMock.Object);
        services.AddSingleton(RemoteServiceExceptionHandlerMock.Object);
        services.AddSingleton(CliVersionServiceMock.Object);
        
        // Simple implementations to avoid dependency issues
        services.AddSingleton<IJsonSerializer>(JsonSerializerMock.Object);
        services.AddSingleton<ICancellationTokenProvider>(new DefaultCancellationTokenProvider());
        services.AddSingleton<CliHttpClientFactory>(new Mock<CliHttpClientFactory>().Object);
        services.AddSingleton<Volo.Abp.Cli.Version.CliVersionService>(CliVersionServiceMock.Object);

        // Register the class under test as transient to match ITransientDependency
        services.AddTransient<AbpIoSourceCodeStore>();

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        ServiceProvider?.Dispose();
    }
}

public class DefaultCancellationTokenProvider : ICancellationTokenProvider
{
    public System.Threading.CancellationToken Token => System.Threading.CancellationToken.None;
}
