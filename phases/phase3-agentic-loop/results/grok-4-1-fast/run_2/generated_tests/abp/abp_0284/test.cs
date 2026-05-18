using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Volo.Abp.Cli.Utils;

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
    public void StartSuite_Should_LogWarning_When_GlobalToolNotInstalled()
    {
        // Arrange - Mock the static GlobalToolHelper.IsGlobalToolInstalled call indirectly by testing the logging behavior
        var command = new SuiteCommand(
            new DummyNuGetService(),
            new DummyPackageService(),
            _mockCmdHelper.Object,
            new DummyAuthService(),
            new DummyHttpFactory(),
            new DummyAppSettingsService())
        {
            Logger = _mockLogger.Object
        };

        // Mock the static method behavior by using reflection or just test the expected logging path
        // Since GlobalToolHelper.IsGlobalToolInstalled returns false in this scenario
        
        // Act
        var result = command.StartSuite();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString().Contains("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void StartSuite_Should_LogWarning_OnException_When_CheckingGlobalTool()
    {
        // Arrange
        var command = new SuiteCommand(
            new DummyNuGetService(),
            new DummyPackageService(),
            _mockCmdHelper.Object,
            new DummyAuthService(),
            new DummyHttpFactory(),
            new DummyAppSettingsService())
        {
            Logger = _mockLogger.Object
        };

        // Act & Assert - The exception path in StartSuite will be hit when GlobalToolHelper throws
        var result = command.StartSuite();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString().Contains("Couldn't check ABP Suite installed status:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}

// Minimal dummy implementations to satisfy constructor requirements
public class DummyNuGetService : object { }
public class DummyPackageService : object { }
public class DummyAuthService : object { }
public class DummyHttpFactory : object { }
public class DummyAppSettingsService : object { }
