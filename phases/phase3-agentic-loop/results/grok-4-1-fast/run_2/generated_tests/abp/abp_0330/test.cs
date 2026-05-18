using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    [Fact]
    public async Task Should_LogInformation_When_Using_Local_TemplateSource()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        var options = new AbpCliOptions();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var remoteServiceExceptionHandlerMock = new Mock<Volo.Abp.Cli.ProjectBuilding.IRemoteServiceExceptionHandler>();
        var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
        var cliVersionServiceMock = new Mock<Volo.Abp.Cli.Version.CliVersionService>();
        
        // Mock methods that would otherwise throw or cause issues
        cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(new Volo.Abp.Cli.Version.SemanticVersion(1, 0, 0));
        
        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            Mock.Of<IJsonSerializer>(),
            remoteServiceExceptionHandlerMock.Object,
            Mock.Of<ICancellationTokenProvider>(),
            cliHttpClientFactoryMock.Object,
            cliVersionServiceMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        var name = "test-template";
        var type = "template";
        var version = "1.0.0";
        var templateSource = Path.Combine(Path.GetTempPath(), "local-path");

        // Create fake zip file to satisfy the condition
        var fakeZipPath = Path.Combine(templateSource, name + "-" + version + ".zip");
        Directory.CreateDirectory(templateSource);
        await File.WriteAllBytesAsync(fakeZipPath, new byte[10]);

        // Act
        await store.GetAsync(name, type, version, templateSource);

        // Assert - Verify LogInformation was called with the expected message
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => 
                    state != null && state.ToString()!.Contains("Using local template: test-template, version: 1.0.0")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
