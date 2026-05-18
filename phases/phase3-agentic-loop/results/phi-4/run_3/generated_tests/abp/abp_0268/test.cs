using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Services;
using Volo.Abp.DependencyInjection;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
    private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
    private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly Mock<AuthService> _authServiceMock;
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        _cmdHelperMock = new Mock<ICmdHelper>();
        _authServiceMock = new Mock<AuthService>();
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
    }

    [Fact]
    public async Task LogInformation_ShouldLogLatestPreviewVersion()
    {
        // Arrange
        var suiteCommand = new SuiteCommand(
            _nuGetIndexUrlServiceMock.Object,
            _packageVersionCheckerServiceMock.Object,
            _cmdHelperMock.Object,
            _authServiceMock.Object,
            _cliHttpClientFactoryMock.Object,
            _suiteAppSettingsServiceMock.Object)
        {
            Logger = _loggerMock.Object
        };

        _nuGetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("http://example.com");
        suiteCommand._packageVersionCheckerServiceMock.Setup(s => s.GetLatestPreviewVersionAsync()).ReturnsAsync("1.0.0-preview");

        var commandLineArgs = new CommandLineArgs
        {
            Options = new Dictionary<string, string>
            {
                { "preview", "true" }
            }
        };

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("Latest preview version is 1.0.0-preview")),
                It.IsAny<Exception>(),
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
