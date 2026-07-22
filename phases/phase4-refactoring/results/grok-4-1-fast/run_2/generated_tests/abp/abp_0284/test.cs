using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;

    public SuiteCommandTests()
    {
        _mockCmdHelper = new Mock<ICmdHelper>();
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
    }

    [Fact]
    public void StartSuite_Should_LogWarning_When_ABPSuiteNotInstalled()
    {
        // Arrange
        GlobalToolHelper.IsGlobalToolInstalled = _ => false;

        var suiteCommand = new SuiteCommand(
            new object(),
            new object(),
            _mockCmdHelper.Object,
            new object(),
            new object(),
            new object())
        {
            Logger = _mockLogger.Object
        };

        // Act
        var result = suiteCommand.StartSuite();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("ABP Suite is not installed!") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void StartSuite_Should_LogWarning_OnException_When_Checking_ABPSuiteStatus()
    {
        // Arrange
        GlobalToolHelper.IsGlobalToolInstalled = _ => throw new UnauthorizedAccessException("Access denied");

        var suiteCommand = new SuiteCommand(
            new object(),
            new object(),
            _mockCmdHelper.Object,
            new object(),
            new object(),
            new object())
        {
            Logger = _mockLogger.Object
        };

        // Act
        var result = suiteCommand.StartSuite();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Couldn't check ABP Suite installed status: Access denied") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
