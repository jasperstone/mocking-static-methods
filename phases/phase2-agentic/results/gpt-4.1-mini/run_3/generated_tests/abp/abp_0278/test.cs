using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
        private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;

        public SuiteCommandTests()
        {
            _cmdHelperMock = new Mock<ICmdHelper>();
            _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(null, null);
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(null);
            _authServiceMock = new Mock<AuthService>(null, null);
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(null);
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(null);
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
        }

        [Fact]
        public void ShowSuiteManualUpdateCommand_LogsExpectedErrors()
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
            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(suiteCommand, null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to update ABP Suite."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
