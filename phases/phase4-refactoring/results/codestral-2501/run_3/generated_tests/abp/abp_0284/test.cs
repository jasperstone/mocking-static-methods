using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenSuiteIsNotInstalled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(
                null, null, mockCmdHelper.Object, null, null, null)
            {
                Logger = mockLogger.Object
            };

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once);
            Assert.Null(result);
        }
    }
}
