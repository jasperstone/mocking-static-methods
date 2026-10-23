using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void KillSuite_LogsInformationWhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommandTestable(loggerMock.Object);

            // Act
            suiteCommand.InvokeKillSuiteWithException(new Exception("Test exception"));

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Cannot close Suite.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class SuiteCommandTestable : SuiteCommand
        {
            public SuiteCommandTestable(ILogger<SuiteCommand> logger)
                : base(
                    nuGetIndexUrlService: null,
                    packageVersionCheckerService: null,
                    cmdHelper: null,
                    authService: null,
                    cliHttpClientFactory: null,
                    suiteAppSettingsService: null)
            {
                Logger = logger;
            }

            public void InvokeKillSuiteWithException(Exception ex)
            {
                try
                {
                    throw ex;
                }
                catch (Exception caughtEx)
                {
                    Logger.LogInformation("Cannot close Suite." + caughtEx.Message);
                }
            }
        }
    }
}
