using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsWarning_WhenAbpSuiteIsNotInstalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = new SuiteCommand(
            null, null, null, null, null, null
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        suiteCommand.StartSuite();

        // Assert
        loggerMock.Verify(
            x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
            Times.Once
        );
    }
}
