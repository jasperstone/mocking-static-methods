using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenGlobalToolNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlService: null,
                packageVersionCheckerService: null,
                cmdHelper: null,
                authService: null,
                cliHttpClientFactory: null,
                suiteAppSettingsService: null)
            {
                Logger = loggerMock.Object
            };

            // Act
            // We cannot call StartSuite directly because it is private.
            // Instead, we test the public ExecuteAsync method with "generate" operation type,
            // which calls StartSuite internally.
            // We expect the warning log if the global tool is not installed.
            // However, since GlobalToolHelper.IsGlobalToolInstalled is static and not mockable,
            // this test will only verify that the warning log is called if the tool is not installed.
            // So we just call StartSuite via reflection here for demonstration.

            var startSuiteMethod = typeof(SuiteCommand).GetMethod("StartSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(startSuiteMethod);

            var process = startSuiteMethod.Invoke(suiteCommand, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite is not installed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            Assert.Null(process);
        }
    }
}
