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
            .Returns(new MockAsyncDisposable());
        
        _cleanCommand = new CleanCommand(_mockCmdHelper.Object, _mockTelemetryService.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_BinObjFoldersRemovedSuccessfully()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDir);
        
        try
        {
            Directory.SetCurrentDirectory(testDir);
            
            // Create test bin/obj directories
            Directory.CreateDirectory(Path.Combine(testDir, "bin"));
            Directory.CreateDirectory(Path.Combine(testDir, "obj"));
            
            _mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert - Verify the specific LogInformation call on line 55
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("'bin' and 'obj' folders removed successfully!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        finally
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetTempPath());
                Directory.Delete(testDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_AllExpectedMessages()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDir);
        
        try
        {
            Directory.SetCurrentDirectory(testDir);
            _mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert - Verify all expected log messages using low-level Log verification
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleaning the solution with 'dotnet clean' command...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Removing 'bin' and 'obj' folders...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("'bin' and 'obj' folders removed successfully!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solution cleaned successfully!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        finally
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetTempPath());
                Directory.Delete(testDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

public class MockAsyncDisposable : IAsyncDisposable
{
    public ValueTask DisposeAsync() => default;
}
