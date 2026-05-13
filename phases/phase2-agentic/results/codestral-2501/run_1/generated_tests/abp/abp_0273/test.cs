using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
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
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(MockBehavior.Strict);
            _suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                null,
                _cmdHelperMock.Object,
                null,
                null,
                null)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogSuccessMessage_WhenExitCodeIsZero()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
                .Callback((string cmd, out int exitCode, string workingDirectory) => exitCode = 0);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("ABP Suite has been successfully installed."),
                Times.Once);
            _loggerMock.Verify(
                x => x.LogInformation("You can run it with the CLI command \"abp suite\""),
                Times.Once);
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogErrorMessage_WhenExitCodeIsNotZero()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
                .Callback((string cmd, out int exitCode, string workingDirectory) => exitCode = 1);

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("You can also run the following command to install ABP Suite."),
                Times.Once);
            _loggerMock.Verify(
                x => x.LogInformation("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                Times.Once);
        }

        [Fact]
        public async Task InstallSuiteAsync_ShouldLogErrorMessage_WhenExceptionIsThrown()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Act
            await _suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            _loggerMock.Verify(
                x => x.LogError("Couldn't install ABP Suite.Test exception"),
                Times.Once);
            _loggerMock.Verify(
                x => x.LogInformation("You can also run the following command to install ABP Suite."),
                Times.Once);
            _loggerMock.Verify(
                x => x.LogInformation("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                Times.Once);
        }
    }
}
