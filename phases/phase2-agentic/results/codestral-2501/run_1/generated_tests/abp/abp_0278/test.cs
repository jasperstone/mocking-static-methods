using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
    private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
    private readonly Mock<AuthService> _authServiceMock;
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
    private readonly SuiteCommand _suiteCommand;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _cmdHelperMock = new Mock<ICmdHelper>();
        _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(MockBehavior.Strict, new Mock<IApiKeyService>().Object);
        _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        _authServiceMock = new Mock<AuthService>();
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(_cmdHelperMock.Object);

        _suiteCommand = new SuiteCommand(
            _nuGetIndexUrlServiceMock.Object,
            _packageVersionCheckerServiceMock.Object,
            _cmdHelperMock.Object,
            _authServiceMock.Object,
            _cliHttpClientFactoryMock.Object,
            _suiteAppSettingsServiceMock.Object)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogError_WhenUpdateSuiteFails()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs { Target = "update" };
        _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns("").Callback((string cmd, out int exitCode) => exitCode = 1);

        // Act
        await _suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogError_WhenUpdateSuiteThrowsException()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs { Target = "update" };
        _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Throws(new Exception("Test exception"));

        // Act
        await _suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
    }
}
