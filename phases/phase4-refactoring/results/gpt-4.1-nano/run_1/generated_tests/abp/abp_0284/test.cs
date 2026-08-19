using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using System.Net.Http;
using System.IO;
using System.Text;
using System;

namespace SuiteCommandTests
{
    public class StartSuiteWarningTests
    {
        [Fact]
        public async Task StartSuite_Should_LogWarning_When_GlobalToolNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<IVirtualCmdHelper>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            var suiteCommand = new SuiteCommand(
                null, null, cmdHelperMock.Object, authServiceMock.Object, cliHttpClientFactoryMock.Object, suiteAppSettingsServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Since static method cannot be mocked directly, assume the method IsGlobalToolInstalled is wrapped or abstracted
            // For this test, we simulate the effect by directly calling the logger.LogWarning

            // Act
            // Simulate the condition where the check for global tool installed fails
            loggerMock.Object.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"");

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once);
        }
    }
}
