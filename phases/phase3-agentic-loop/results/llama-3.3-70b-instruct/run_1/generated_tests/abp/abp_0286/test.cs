using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsError_WhenPortIsAlreadyInUse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                cmdHelperMock.Object,
                new AuthService(),
                new CliHttpClientFactory(),
                new SuiteAppSettingsService()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            suiteCommand._abpSuitePort = 3000;
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();
            tcpListeners.Add(new IPEndPoint(IPAddress.Loopback, 3000));
            suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void StartSuite_ReturnsNull_WhenPortIsAlreadyInUse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                cmdHelperMock.Object,
                new AuthService(),
                new CliHttpClientFactory(),
                new SuiteAppSettingsService()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            suiteCommand._abpSuitePort = 3000;
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();
            tcpListeners.Add(new IPEndPoint(IPAddress.Loopback, 3000));
            var result = suiteCommand.StartSuite();

            // Assert
            Assert.Null(result);
        }
    }
}
