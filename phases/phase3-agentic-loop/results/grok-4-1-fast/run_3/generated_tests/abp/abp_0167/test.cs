using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        
        _cleanCommand = new CleanCommand(_mockCmdHelper.Object, _mockTelemetryService.Object);
        _cleanCommand.Logger = _mockLogger.Object;
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_SuccessMessage_OnLine55()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(new Dictionary<string, string>());
        
        // Mock telemetry - return a mock IAsyncDisposable that does nothing
        var mockDisposable = new Mock<IAsyncDisposable>();
        mockDisposable.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _mockTelemetryService.Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .ReturnsAsync(mockDisposable.Object);
        
        // Mock CmdHelper
        _mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()));

        // Use temp directory with no bin/obj folders
        var originalCurrentDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        Directory.SetCurrentDirectory(tempDir);
        
        try
        {
            // Act
            await _cleanCommand.ExecuteAsync(commandLineArgs);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDir);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        // Assert - Verify the specific LogInformation call on line 55
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("'bin' and 'obj' folders removed successfully!")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
