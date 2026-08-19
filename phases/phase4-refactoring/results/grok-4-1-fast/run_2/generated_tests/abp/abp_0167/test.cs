using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
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
            .Returns(new Mock<IAsyncDisposable>().Object);
        
        _mockCmdHelper
            .Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();
        
        _cleanCommand = new CleanCommand(_mockCmdHelper.Object, _mockTelemetryService.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_BinObjFoldersRemovedSuccessfully()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs();
        
        // Create test directories to ensure the loop runs
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.SetCurrentDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "bin"));
            Directory.CreateDirectory(Path.Combine(tempDir, "obj"));

            // Act
            await _cleanCommand.ExecuteAsync(commandLineArgs);

            // Assert - specifically test the line 55 call
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("'bin' and 'obj' folders removed successfully!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void GetUsageInfo_Should_Return_Expected_Usage()
    {
        // Act
        var usageInfo = _cleanCommand.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", usageInfo);
        Assert.Contains("abp clean", usageInfo);
        Assert.Contains("https://abp.io/docs/latest/cli", usageInfo);
    }

    [Fact]
    public void GetShortDescription_Should_Return_Expected_Description()
    {
        // Act
        var description = CleanCommand.GetShortDescription();

        // Assert
        Assert.Equal("Delete all BIN and OBJ folders in current folder.", description);
    }
}
