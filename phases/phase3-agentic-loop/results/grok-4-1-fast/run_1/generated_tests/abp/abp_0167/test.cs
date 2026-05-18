using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public async Task ExecuteAsync_Should_Log_BinObjFoldersRemovedSuccessfully()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs();
        
        // Mock telemetry using Task.FromResult
        var mockDisposable = new Mock<IAsyncDisposable>();
        _mockTelemetryService
            .Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(mockDisposable.Object));

        // Mock CmdHelper
        _mockCmdHelper
            .Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        // Mock Directory static methods to avoid real file system access
        var mockDirectory = new MockDirectory();
        mockDirectory.EnumerateDirectoriesReturns = Enumerable.Empty<string>();
        DirectoryShim.SetDirectory(mockDirectory);

        // Act
        await _cleanCommand.ExecuteAsync(commandLineArgs);

        // Assert - Verify the specific LogInformation call (line 55)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("'bin' and 'obj' folders removed successfully!")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetUsageInfo_Should_Return_Expected_String()
    {
        // Act
        var result = _cleanCommand.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", result);
        Assert.Contains("abp clean", result);
        Assert.Contains("https://abp.io/docs/latest/cli", result);
    }

    [Fact]
    public void GetShortDescription_Should_Return_Expected_String()
    {
        // Act
        var result = CleanCommand.GetShortDescription();

        // Assert
        Assert.Equal("Delete all BIN and OBJ folders in current folder.", result);
    }
}

// Shim classes to mock static Directory methods
public static class DirectoryShim
{
    public static MockDirectory Instance { get; private set; } = new MockDirectory();

    public static void SetDirectory(MockDirectory mockDirectory)
    {
        Instance = mockDirectory;
    }
}

public class MockDirectory
{
    public Func<string, string, SearchOption, IEnumerable<string>> EnumerateDirectoriesFunc = 
        (_, _, _) => Enumerable.Empty<string>();
    public IEnumerable<string> EnumerateDirectoriesReturns = Enumerable.Empty<string>();

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        return EnumerateDirectoriesFunc(path, searchPattern, searchOption) ?? EnumerateDirectoriesReturns;
    }
}
