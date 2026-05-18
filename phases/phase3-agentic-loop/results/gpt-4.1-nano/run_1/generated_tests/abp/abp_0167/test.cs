using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

public class CleanCommandTests
{
    [Fact]
    public async Task ExecuteAsync_Should_LogInformation_Calls()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CleanCommand>>();
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockTelemetryService = new Mock<ITelemetryService>();

        // Setup telemetry to return a disposable
        mockTelemetryService.Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
            .ReturnsAsync(new DummyDisposable());

        var command = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object);
        command.Logger = mockLogger.Object;

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(
            x => x.LogInformation("Cleaning the solution with 'dotnet clean' command..."),
            Times.Once);
        mockLogger.Verify(
            x => x.LogInformation($"Removing 'bin' and 'obj' folders..."),
            Times.Once);
        mockLogger.Verify(
            x => x.LogInformation($"'bin' and 'obj' folders removed successfully!"),
            Times.Once);
        mockLogger.Verify(
            x => x.LogInformation("Solution cleaned successfully!"),
            Times.Once);
    }

    private class DummyDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => new ValueTask();
    }
}
