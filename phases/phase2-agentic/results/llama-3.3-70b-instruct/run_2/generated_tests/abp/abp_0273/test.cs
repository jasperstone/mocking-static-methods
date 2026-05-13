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
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();

            _suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                new Mock<PackageVersionCheckerService>().Object,
                _cmdHelperMock.Object,
                new Mock<AuthService>().Object,
                new Mock<CliHttpClientFactory>().Object,
                new Mock<SuiteAppSettingsService>().Object
            );
            _suiteCommand.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task ExecuteAsync_InstallSuiteIfNotInstalledAsync_LogsInformation()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs { Target = "" };
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
                .Callback<string, int, string>((cmd, exitCode, workingDirectory) => exitCode = 0);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_InstallSuiteAsync_LogsInformation()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs { Target = "install" };
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
                .Callback<string, int, string>((cmd, exitCode, workingDirectory) => exitCode = 0);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_UpdateSuiteAsync_LogsInformation()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs { Target = "update" };
            _nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://nuget.abp.io/<your-private-key>/v3/index.json");

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ShowSuiteManualInstallCommand_LogsInformation()
        {
            // Act
            _suiteCommand.ShowSuiteManualInstallCommand();

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
