using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
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
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
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
                _suiteAppSettingsServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_OperationTypeIsEmpty()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "",
                Options = new System.Collections.Generic.Dictionary<string, string>()
            };
            _authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(new LoginInfo { Organization = "org" });
            _suiteAppSettingsServiceMock.Setup(s => s.GetSuitePortAsync(It.IsAny<string>())).ReturnsAsync(3000);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_OperationTypeIsGenerate()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "generate",
                Options = new System.Collections.Generic.Dictionary<string, string>()
            };
            _authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(new LoginInfo { Organization = "org" });
            _suiteAppSettingsServiceMock.Setup(s => s.GetSuitePortAsync(It.IsAny<string>())).ReturnsAsync(3000);
            var mockClient = new Mock<HttpClient>();
            _cliHttpClientFactoryMock.Setup(c => c.CreateClient(It.IsAny<bool>())).Returns(mockClient.Object);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_OperationTypeIsRemove()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "remove",
                Options = new System.Collections.Generic.Dictionary<string, string>()
            };
            _authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(new LoginInfo { Organization = "org" });
            _suiteAppSettingsServiceMock.Setup(s => s.GetSuitePortAsync(It.IsAny<string>())).ReturnsAsync(3000);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Removing ABP Suite..."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_Throw_When_InvalidOperationType()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "invalid",
                Options = new System.Collections.Generic.Dictionary<string, string>()
            };
            _authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(new LoginInfo { Organization = "org" });
            _suiteAppSettingsServiceMock.Setup(s => s.GetSuitePortAsync(It.IsAny<string>())).ReturnsAsync(3000);

            // Act & Assert
            await Assert.ThrowsAsync<CliUsageException>(() => _suiteCommand.ExecuteAsync(commandLineArgs));
        }
    }
}
