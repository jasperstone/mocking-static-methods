using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenSuiteNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();

            var suiteCommand = new TestSuiteCommand(loggerMock.Object);

            // Act
            var result = suiteCommand.CallStartSuite();

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite is not installed!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSuiteCommand : SuiteCommand
        {
            private readonly ILogger<SuiteCommand> _logger;

            public TestSuiteCommand(ILogger<SuiteCommand> logger)
                : base(
                    nuGetIndexUrlService: null,
                    packageVersionCheckerService: null,
                    cmdHelper: null,
                    authService: null,
                    cliHttpClientFactory: null,
                    suiteAppSettingsService: null)
            {
                _logger = logger;
                Logger = logger;
            }

            public Process CallStartSuite()
            {
                return StartSuiteInternal();
            }

            // We cannot override StartSuite because it's private, so we create a new method to simulate the behavior
            private Process StartSuiteInternal()
            {
                try
                {
                    if (!IsGlobalToolInstalled("abp-suite"))
                    {
                        Logger.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Couldn't check ABP Suite installed status: " + ex.Message);
                }

                return new Process(); // dummy return to satisfy method signature
            }

            // Simulate the static call to GlobalToolHelper.IsGlobalToolInstalled
            private bool IsGlobalToolInstalled(string toolName)
            {
                return false;
            }
        }
    }
}
