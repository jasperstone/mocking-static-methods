using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class CleanCommandTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly Mock<ILogger<CleanCommand>> _mockLogger;
    private readonly CleanCommand _cleanCommand;

    public CleanCommandTests()
    {
        _mockCmdHelper = new Mock<ICmdHelper>();
        _mockLogger = new Mock<ILogger<CleanCommand>>();
        _cleanCommand = new CleanCommand(_mockCmdHelper.Object, Mock.Of<Volo.Abp.Internal.Telemetry.ITelemetryService>())
        {
            Logger = _mockLogger.Object
        };

        _mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()));
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_BinObjFoldersRemovedSuccessfully()
    {
        // Act
        await _cleanCommand.ExecuteAsync(new CommandLineArgs());

        // Assert - specifically testing the LogInformation call on line ~55
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'bin' and 'obj' folders removed successfully!")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_SolutionCleanedSuccessfully()
    {
        // Act
        await _cleanCommand.ExecuteAsync(new CommandLineArgs());

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Solution cleaned successfully!")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Call_DotnetClean()
    {
        // Act
        await _cleanCommand.ExecuteAsync(new CommandLineArgs());

        // Assert
        _mockCmdHelper.Verify(x => x.RunCmd("dotnet clean", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetUsageInfo_Should_Return_Expected_Usage()
    {
        // Act
        var result = _cleanCommand.GetUsageInfo();

        // Assert
        Assert.Contains("abp clean", result);
        Assert.Contains("https://abp.io/docs/latest/cli", result);
    }

    [Fact]
    public void GetShortDescription_Should_Return_Expected_Description()
    {
        // Act
        var result = CleanCommand.GetShortDescription();

        // Assert
        Assert.Equal("Delete all BIN and OBJ folders in current folder.", result);
    }
}
