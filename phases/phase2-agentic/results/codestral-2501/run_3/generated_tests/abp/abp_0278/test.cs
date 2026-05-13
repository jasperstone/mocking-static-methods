using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly SuiteCommand _suiteCommand;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _cmdHelperMock = new Mock<ICmdHelper>();

        _suiteCommand = new SuiteCommand(
            null,
            null,
            _cmdHelperMock.Object,
            null,
            null,
            null)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_ShouldLogError()
    {
        // Act
        _suiteCommand.ShowSuiteManualUpdateCommand();

        // Assert
        _loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateSuiteAsync_ShouldLogError_WhenCmdHelperThrowsException()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs();
        _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
            .Throws(new Exception("Test exception"));

        // Act
        await _suiteCommand.UpdateSuiteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
