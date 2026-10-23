using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Args;
using System.Threading.Tasks;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsWarning_WhenAbpSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(
                null, null, cmdHelperMock.Object, null, null, null)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs("suite");

            // Act
            await suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once);
        }

        [Fact]
        public void IsGlobalToolInstalled_ReturnsFalse_WhenToolIsNotInstalled()
        {
            // Arrange
            var toolCommandName = "non-existent-tool";

            // Act
            var result = GlobalToolHelper.IsGlobalToolInstalled(toolCommandName);

            // Assert
            Assert.False(result);
        }
    }
}
