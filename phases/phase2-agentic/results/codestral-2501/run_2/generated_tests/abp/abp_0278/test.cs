using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public void ShowSuiteManualUpdateCommand_LogsCorrectErrorMessages()
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
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        suiteCommand.ShowSuiteManualUpdateCommand();

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You can also run the following command to update ABP Suite.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogError(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async void UpdateSuiteAsync_LogsErrorWhenExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Throws(new Exception("Test exception"));
        var suiteCommand = new SuiteCommand(
            null,
            null,
            cmdHelperMock.Object,
            null,
            null,
            null
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await suiteCommand.UpdateSuiteAsync(null, false);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Couldn't update ABP Suite.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void RunSuite_LogsWarningWhenGlobalToolNotInstalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var globalToolHelperMock = new Mock<GlobalToolHelper>();
        globalToolHelperMock.Setup(x => x.IsGlobalToolInstalled("abp-suite")).Returns(false);
        var suiteCommand = new SuiteCommand(
            null,
            null,
            null,
            null,
            null,
            null
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        suiteCommand.RunSuite(null);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
