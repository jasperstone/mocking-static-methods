using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
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
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(_cmdHelperMock.Object);

            _suiteCommand = new SuiteCommand(
                null,
                null,
                _cmdHelperMock.Object,
                null,
                null,
                _suiteAppSettingsServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void StartSuite_WhenSuiteIsNotInstalled_LogsWarning()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns("");

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void StartSuite_WhenSuiteIsInstalledAndNotRunningAndPortIsNotInUse_StartsSuite()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns("volo.abp.suite 1.0.0");
            _cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns(new Process());

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.Never);
            Assert.NotNull(result);
        }

        [Fact]
        public void StartSuite_WhenSuiteIsInstalledAndRunning_LogsWarning()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns("volo.abp.suite 1.0.0");
            _cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns((Process)null);

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.Never);
            Assert.Null(result);
        }

        [Fact]
        public void StartSuite_WhenPortIsInUse_LogsError()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns("volo.abp.suite 1.0.0");
            _cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns((Process)null);
            _suiteAppSettingsServiceMock.Setup(x => x.GetSuitePortAsync(It.IsAny<string>())).ReturnsAsync(3000);

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogError($"Port \"3000\" is already in use."),
                Times.Once);
            Assert.Null(result);
        }
    }
}
