using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Licensing;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
        private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly Mock<IApiKeyService> _apiKeyServiceMock;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(MockBehavior.Strict, new Mock<IApiKeyService>().Object);
            _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _authServiceMock = new Mock<AuthService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            _apiKeyServiceMock = new Mock<IApiKeyService>();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenSuiteIsInstalled()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("suite", new Dictionary<string, string>());
            var suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };

            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(true);

            // Act
            await suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("ABP Suite has been successfully installed."),
                Times.Once
            );
            _loggerMock.Verify(
                x => x.LogInformation("You can run it with the CLI command \"abp suite\""),
                Times.Once
            );
        }

        [Fact]
        public void ShowSuiteManualInstallCommand_ShouldLogInformation()
        {
            // Arrange
            var suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };

            // Act
            suiteCommand.ShowSuiteManualInstallCommand();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("You can also run the following command to install ABP Suite."),
                Times.Once
            );
            _loggerMock.Verify(
                x => x.LogInformation("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                Times.Once
            );
        }
    }
}
