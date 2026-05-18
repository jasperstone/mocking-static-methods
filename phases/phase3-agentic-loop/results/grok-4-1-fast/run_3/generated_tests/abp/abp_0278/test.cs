using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Utils;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;

    [Fact]
    public void ShowSuiteManualUpdateCommand_ShouldCallLogError_Twice()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        var suiteCommand = CreateSuiteCommand(_loggerMock.Object, cmdHelperMock.Object);

        // Act
        suiteCommand.ShowSuiteManualUpdateCommand();

        // Assert - Verify first LogError call (line ~410)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You can also run the following command to update ABP Suite.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Assert - Verify second LogError call with dotnet command
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool update -g Volo.Abp.Suite")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void UpdateSuiteAsync_ExceptionPath_ShouldCallShowSuiteManualUpdateCommand()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
                    .Throws(new Exception("Test exception"));
        
        var suiteCommand = CreateSuiteCommand(_loggerMock.Object, cmdHelperMock.Object);

        // Act & Assert - This exercises the catch block that calls ShowSuiteManualUpdateCommand
        // which contains the LogError calls on line ~410 and beyond
        Assert.Throws<Exception>(() => suiteCommand.UpdateSuiteAsync(null, false));
        
        // Verify ShowSuiteManualUpdateCommand logging was triggered
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeast(2));
    }

    private SuiteCommand CreateSuiteCommand(ILogger<SuiteCommand> logger, ICmdHelper cmdHelper)
    {
        var mockNugetService = new Mock<object>().Object;
        var mockVersionService = new Mock<object>().Object;
        var mockAuthService = new Mock<object>().Object;
        var mockHttpFactory = new Mock<object>().Object;
        var mockSettingsService = new Mock<object>().Object;

        var command = new SuiteCommand(
            mockNugetService,
            mockVersionService,
            cmdHelper,
            mockAuthService,
            mockHttpFactory,
            mockSettingsService);
        
        command.Logger = logger;
        return command;
    }
}
