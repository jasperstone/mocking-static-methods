using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async void ExecuteAsync_LogsWarning_WhenSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(
                null, null, cmdHelperMock.Object, null, null, null)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs(new string[] { "suite" });

            // Act
            await suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once);
        }

        [Fact]
        public async void ExecuteAsync_LogsWarning_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(
                null, null, cmdHelperMock.Object, null, null, null)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs(new string[] { "suite" });

            // Act
            await suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.Once);
        }
    }
}
