using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Volo.Abp.Cli.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenAbpSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var globalToolHelperMock = new Mock<GlobalToolHelper>();
            globalToolHelperMock.Setup(x => x.IsGlobalToolInstalled("abp-suite")).Returns(false);
            var result = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once
            );
        }

        [Fact]
        public void StartSuite_LogsWarning_WhenCheckingAbpSuiteInstalledStatusFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var globalToolHelperMock = new Mock<GlobalToolHelper>();
            globalToolHelperMock.Setup(x => x.IsGlobalToolInstalled("abp-suite")).Throws(new Exception("Test exception"));
            var result = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Couldn't check ABP Suite installed status: Test exception"),
                Times.Once
            );
        }
    }
}
