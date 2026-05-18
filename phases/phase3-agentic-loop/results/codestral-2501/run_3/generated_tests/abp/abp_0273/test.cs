using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();

            _suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                _cmdHelperMock.Object,
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogSuccessMessage_WhenInstallationSucceeds()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
                .Callback((string cmd, out int exitCode, string workingDirectory) => exitCode = 0);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite has been successfully installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogErrorMessage_WhenInstallationFails()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
                .Callback((string cmd, out int exitCode, string workingDirectory) => exitCode = 1);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Couldn't install ABP Suite.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void ShowSuiteManualInstallCommand_ShouldLogManualInstallCommand()
        {
            // Act
            _suiteCommand.ShowSuiteManualInstallCommand();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
