using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests.Commands
{
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
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
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
        public async Task InstallSuite_WhenSuccessful_LogsSuccessMessages()
        {
            // Arrange
            _cmdHelperMock.Setup(c => c.RunCmd(It.IsAny<string>(), out int exitCode))
                .Callback((string cmd, out int exitCode) =>
                {
                    exitCode = 0; // Simulate successful command execution
                });

            // Act
            await _suiteCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation("ABP Suite has been successfully installed."),
                Times.Once);

            _loggerMock.Verify(
                l => l.LogInformation("You can run it with the CLI command \"abp suite\""),
                Times.Once);
        }

        [Fact]
        public async Task InstallSuite_WhenFailed_LogsErrorMessage()
        {
            // Arrange
            _cmdHelperMock.Setup(c => c.RunCmd(It.IsAny<string>(), out int exitCode))
                .Callback((string cmd, out int exitCode) =>
                {
                    exitCode = 1; // Simulate failed command execution
                });

            // Act
            await _suiteCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                l => l.LogError("Couldn't install ABP Suite."),
                Times.Once);

            _loggerMock.Verify(
                l => l.LogInformation("You can also run the following command to install ABP Suite."),
                Times.Once);

            _loggerMock.Verify(
                l => l.LogInformation("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                Times.Once);
        }
    }
}
