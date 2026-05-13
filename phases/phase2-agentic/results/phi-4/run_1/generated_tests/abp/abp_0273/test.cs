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
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            var mockNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>();
            var mockPackageVersionCheckerService = new Mock<PackageVersionCheckerService>();
            var mockAuthService = new Mock<AuthService>();
            var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockSuiteAppSettingsService = new Mock<SuiteAppSettingsService>();

            _suiteCommand = new SuiteCommand(
                mockNuGetIndexUrlService.Object,
                mockPackageVersionCheckerService.Object,
                new CmdHelper(),
                mockAuthService.Object,
                mockCliHttpClientFactory.Object,
                mockSuiteAppSettingsService.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task InstallSuite_WhenSuccessful_LogsSuccessMessages()
        {
            // Arrange
            var mockNuGetIndexUrlService = _suiteCommand._nuGetIndexUrlService as Mock<AbpNuGetIndexUrlService>;
            mockNuGetIndexUrlService.Setup(s => s.GetAsync()).ReturnsAsync("https://nuget.abp.io");

            // Act
            await _suiteCommand.InstallSuiteAsync();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("ABP Suite has been successfully installed.")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );

            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("You can run it with the CLI command \"abp suite\"")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task InstallSuite_WhenFailed_LogsErrorMessage()
        {
            // Arrange
            var mockNuGetIndexUrlService = _suiteCommand._nuGetIndexUrlService as Mock<AbpNuGetIndexUrlService>;
            mockNuGetIndexUrlService.Setup(s => s.GetAsync()).ReturnsAsync("https://nuget.abp.io");

            // Simulate failure by throwing an exception
            var exception = new Exception("Installation failed");
            _suiteCommand.CmdHelper = new CmdHelper
            {
                RunCmd = (command, exitCode) =>
                {
                    throw exception;
                }
            };

            // Act
            await _suiteCommand.InstallSuiteAsync();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Couldn't install ABP Suite.")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void ShowSuiteManualInstallCommand_LogsManualInstallCommand()
        {
            // Act
            _suiteCommand.ShowSuiteManualInstallCommand();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}
