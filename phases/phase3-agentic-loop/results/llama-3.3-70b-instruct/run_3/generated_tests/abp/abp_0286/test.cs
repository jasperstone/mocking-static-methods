using Xunit;
using Moq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_PortAlreadyInUse_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(null, null, cmdHelperMock.Object, null, null, null);
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void StartSuite_PortNotInUse_StartsSuite()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(x => x.RunCmdAndGetProcess(It.IsAny<string>())).Returns(new Process());
            var suiteCommand = new SuiteCommand(null, null, cmdHelperMock.Object, null, null, null);
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            Assert.NotNull(result);
        }
    }
}
