using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class CleanCommandTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly Mock<ITelemetryService> _mockTelemetryService;
    private readonly Mock<ILogger<CleanCommand>> _mockLogger;
    private readonly CleanCommand _cleanCommand;

    public CleanCommandTests()
    {
        _mockCmdHelper = new Mock<ICmdHelper>();
        _mockTelemetryService = new Mock<ITelemetryService>();
        _mockLogger = new Mock<ILogger<CleanCommand>>();
        
        _mockTelemetryService
            .Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .Returns(new FakeAsyncDisposable());
        
        _cleanCommand = new CleanCommand(_mockCmdHelper.Object, _mockTelemetryService.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_BinAndObjFoldersRemovedSuccessfully()
    {
        // Arrange
        var commandLineArgs = CommandLineArgs.Empty();
        SetupEmptyDirectories();

        // Act
        await _cleanCommand.ExecuteAsync(commandLineArgs);

        // Assert - Verify the specific LogInformation call on line ~55
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("'bin' and 'obj' folders removed successfully!")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_AllExpectedMessages()
    {
        // Arrange
        var commandLineArgs = CommandLineArgs.Empty();
        SetupEmptyDirectories();

        // Act
        await _cleanCommand.ExecuteAsync(commandLineArgs);

        // Assert - Verify all LogInformation calls using low-level Log method
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleaning the solution with 'dotnet clean' command...")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Removing 'bin' and 'obj' folders...")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("'bin' and 'obj' folders removed successfully!")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solution cleaned successfully!")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void GetUsageInfo_Should_ReturnExpectedUsageText()
    {
        // Act
        var usageInfo = _cleanCommand.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", usageInfo);
        Assert.Contains("abp clean", usageInfo);
        Assert.Contains("https://abp.io/docs/latest/cli", usageInfo);
    }

    [Fact]
    public void GetShortDescription_Should_ReturnExpectedDescription()
    {
        // Act
        var description = CleanCommand.GetShortDescription();

        // Assert
        Assert.Equal("Delete all BIN and OBJ folders in current folder.", description);
    }

    private void SetupEmptyDirectories()
    {
        _mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();
    }
}

public class FakeAsyncDisposable : IAsyncDisposable
{
    public ValueTask DisposeAsync() => default;
}
