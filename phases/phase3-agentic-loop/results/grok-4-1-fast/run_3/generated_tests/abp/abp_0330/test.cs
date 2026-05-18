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
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private bool _logInformationCalled;

    [Fact]
    public async Task Should_LogInformation_When_Using_Local_NonNetwork_TemplateSource()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        loggerMock.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, eventId, state, ex, formatter) =>
            {
                _logInformationCalled = true;
            });

        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

        var jsonSerializerMock = new Mock<Volo.Abp.Json.IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
        var cliVersionServiceMock = new Mock<Volo.Abp.Cli.Version.CliVersionService>();
        cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(new Volo.Abp.Cli.Version.SemanticVersion(1, 0, 0));

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

        var name = "test-template";
        var type = "template";
        var version = "1.0.0";
        var templateSource = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);

        try
        {
            Directory.CreateDirectory(templateSource);
            var fakeZipPath = Path.Combine(templateSource, name + "-" + version + ".zip");
            await File.WriteAllBytesAsync(fakeZipPath, new byte[10]);

            // Mock dependencies to avoid real calls
            jsonSerializerMock.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");
            remoteServiceExceptionHandlerMock.Setup(x => x.HandleError(It.IsAny<Exception>()));
            cancellationTokenProviderMock.Setup(x => x.Token).Returns(default(CancellationToken));

            // Act
            await store.GetAsync(name, type, version, templateSource);

            // Assert
            Assert.True(_logInformationCalled);
            loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
        finally
        {
            if (Directory.Exists(templateSource))
            {
                try { Directory.Delete(templateSource, true); } catch { }
            }
        }
    }
}
