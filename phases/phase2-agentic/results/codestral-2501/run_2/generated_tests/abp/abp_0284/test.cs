using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
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
        public void StartSuite_WhenAbpSuiteIsNotInstalled_LogsWarning()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns((Process)null);

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""))),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void StartSuite_WhenAbpSuiteIsInstalledAndNotRunning_StartsSuite()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns(new Process());

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""))),
                Times.Never);
            Assert.NotNull(result);
        }

        [Fact]
        public void StartSuite_WhenPortIsAlreadyInUse_LogsError()
        {
            // Arrange
            var ipGlobalPropertiesMock = new Mock<IPGlobalProperties>();
            ipGlobalPropertiesMock.Setup(x => x.GetActiveTcpListeners()).Returns(new IPEndPoint[] { new IPEndPoint(0, 3000) });
            _cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns(new Process());

            // Act
            var result = _suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Port \"3000\" is already in use."))),
                Times.Once);
            Assert.Null(result);
        }
    }
}
