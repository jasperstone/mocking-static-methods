using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
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

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            _authServiceMock = new Mock<AuthService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
        }

        [Fact]
        public async Task ExecuteAsync_InstallSuite_LogsInformation()
        {
            // Arrange
            var suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            );
            suiteCommand.Logger = _loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "install" });

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("ABP Suite has been successfully installed."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_UpdateSuite_LogsInformation()
        {
            // Arrange
            var suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            );
            suiteCommand.Logger = _loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "update" });

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ShowSuiteManualInstallCommand_LogsInformation()
        {
            // Arrange
            var suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            );
            suiteCommand.Logger = _loggerMock.Object;

            // Act
            suiteCommand.ShowSuiteManualInstallCommand();

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
