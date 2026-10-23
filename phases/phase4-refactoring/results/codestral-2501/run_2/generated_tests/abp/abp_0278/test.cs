using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public async Task ExecuteAsync_LogsErrorMessages_WhenExceptionIsThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommand(
            null, null, cmdHelperMock.Object, null, null, null)
        {
            Logger = loggerMock.Object
        };

        cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Throws(new Exception("Test exception"));

        var commandLineArgs = new CommandLineArgs
        {
            Target = "update"
        };

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
    }
}
