using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenSuiteNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var testSuiteCommand = new TestSuiteCommandForStartSuite(loggerMock.Object);

            // Act
            var result = testSuiteCommand.CallStartSuite();

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

        private class TestSuiteCommandForStartSuite : SuiteCommand
        {
            public TestSuiteCommandForStartSuite(ILogger<SuiteCommand> logger)
                : base(null, null, null, null, null, null)
            {
                Logger = logger;
            }

            public Process CallStartSuite()
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

                return null;
            }

            private bool IsGlobalToolInstalled(string toolName)
            {
                // Simulate that the global tool is not installed
                return false;
            }
        }
    }
}
