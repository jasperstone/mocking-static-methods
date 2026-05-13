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
    private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
    private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly Mock<AuthService> _authServiceMock;
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
    private readonly SuiteCommand _suiteCommand;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(MockBehavior.Strict, new Mock<IApiKeyService>().Object);
        _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        _cmdHelperMock = new Mock<ICmdHelper>();
        _authServiceMock = new Mock<AuthService>();
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

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
    public async Task InstallSuiteAsync_ShouldLogSuccessMessage_WhenInstallationSucceeds()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs { Target = "install" };
        _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://nuget.abp.io/v3/index.json");
        _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(0);

        // Act
        await _suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite has been successfully installed.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Exactly(2));
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogErrorMessage_WhenInstallationFails()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs { Target = "install" };
        _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://nuget.abp.io/v3/index.json");
        _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(1);

        // Act
        await _suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You can also run the following command to install ABP Suite.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_ShouldLogManualInstallCommand()
    {
        // Act
        _suiteCommand.InvokePrivateMethod("ShowSuiteManualInstallCommand");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
