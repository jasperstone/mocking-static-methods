using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenAbpSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null,
                null,
                null,
                null,
                null,
                null
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""), Times.Once);
        }

        [Fact]
        public void StartSuite_LogsWarning_WhenCheckingAbpSuiteInstalledStatusFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null,
                null,
                null,
                null,
                null,
                null
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(x => x.LogWarning("Couldn't check ABP Suite installed status: "), Times.Once);
        }
    }
}
