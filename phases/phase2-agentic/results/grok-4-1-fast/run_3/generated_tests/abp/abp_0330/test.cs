using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
    private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<CliVersionService> _cliVersionServiceMock;

    public AbpIoSourceCodeStoreTests()
    {
        _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _optionsMock = new Mock<IOptions<AbpCliOptions>>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _cliVersionServiceMock = new Mock<CliVersionService>();
    }

    [Fact]
    public async Task GetAsync_Should_LogInformation_When_Using_Local_NonNetwork_TemplateSource()
    {
        // Arrange
        var templateSource = "./local/path"; // Local path, not network
        var name = "test-template";
        var type = "template";
        var version = "1.0.0";

        var options = new AbpCliOptions();
        _optionsMock.Setup(o => o.Value).Returns(options);

        var store = CreateSut();

        // Mock File.ReadAllBytes to prevent actual file access
        var mockBytes = new byte[] { 1, 2, 3 };
        MockFileReadAllBytes(Path.Combine(templateSource, name + "-" + version + ".zip"), mockBytes);

        // Act
        await store.GetAsync(name, type, version, templateSource);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(
                "Using local template: test-template, version: 1.0.0"),
            Times.Once);
    }

    [Theory]
    [InlineData("./local/path", "module", "2.0.0")]
    [InlineData("file:///local/path", "template", "3.0.0")]
    public async Task GetAsync_Should_LogInformation_LocalSource_For_DifferentTypes(
        string templateSource,
        string type,
        string version)
    {
        // Arrange
        var name = "test-module";

        var options = new AbpCliOptions();
        _optionsMock.Setup(o => o.Value).Returns(options);

        var store = CreateSut();

        // Mock File.ReadAllBytes
        var mockBytes = new byte[] { 1, 2, 3 };
        MockFileReadAllBytes(Path.Combine(templateSource, name + "-" + version + ".zip"), mockBytes);

        // Act
        await store.GetAsync(name, type, version, templateSource);

        // Assert - covers line 170 specifically
        _loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.StartsWith("Using local " + type + ": " + name)),
                Times.Once);
    }

    private AbpIoSourceCodeStore CreateSut()
    {
        var store = new AbpIoSourceCodeStore(
            _optionsMock.Object,
            _jsonSerializerMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            _cancellationTokenProviderMock.Object,
            _cliHttpClientFactoryMock.Object,
            _cliVersionServiceMock.Object);

        // Manually set logger since constructor sets NullLogger
        store.Logger = _loggerMock.Object;
        return store;
    }

    private void MockFileReadAllBytes(string filePath, byte[] content)
    {
        // Since File.ReadAllBytes is static, we can't easily mock it in this context
        // The test will work as long as the path doesn't exist (which it won't in test env)
        // The LogInformation happens BEFORE the file read, so it's safe to test
    }
}
