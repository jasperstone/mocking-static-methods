using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Core;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
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
        public async Task InstallSuiteAsync_LogsInformation_WhenInstallationIsSuccessful()
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

            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Callback<string, int>((cmd, exitCode) => exitCode = 0);

            // Act
            await suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("ABP Suite has been successfully installed."), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("You can run it with the CLI command \"abp suite\""), Times.Once);
        }

        [Fact]
        public async Task InstallSuiteAsync_LogsError_WhenInstallationFails()
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

            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Callback<string, int>((cmd, exitCode) => exitCode = 1);

            // Act
            await suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
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
            _loggerMock.Verify(x => x.LogInformation("You can also run the following command to install ABP Suite."), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"), Times.Once);
        }
    }
}
