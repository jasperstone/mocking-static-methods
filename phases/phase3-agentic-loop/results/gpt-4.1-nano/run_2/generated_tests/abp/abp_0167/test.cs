using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

public class CleanCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformationCalls()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockTelemetryService = new Mock<ITelemetryService>();
        var mockLogger = new Mock<ILogger<CleanCommand>>();

        // Setup the telemetry service to return a disposable
        mockTelemetryService
            .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
            .ReturnsAsync(Mock.Of<IAsyncDisposable>());

        var command = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object);
        command.Logger = mockLogger.Object;

        // Setup RunCmd to do nothing
        mockCmdHelper
            .Setup(c => c.RunCmd(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        // Create temporary directories for bin and obj
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var binDir = Path.Combine(tempDir, "bin");
        var objDir = Path.Combine(tempDir, "obj");
        Directory.CreateDirectory(binDir);
        Directory.CreateDirectory(objDir);

        // Create nested directories
        Directory.CreateDirectory(Path.Combine(binDir, "nested"));
        Directory.CreateDirectory(Path.Combine(objDir, "nested"));

        // Save current directory and switch to temp
        var originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(tempDir);

        try
        {
            // Act
            await command.ExecuteAsync(null);

            // Assert
            mockLogger.Verify(l => l.LogInformation("Cleaning the solution with 'dotnet clean' command..."), Times.Once);
            mockLogger.Verify(l => l.LogInformation("Removing 'bin' and 'obj' folders..."), Times.Once);
            mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.StartsWith("Deleting:"))), Times.Exactly(2));
            mockLogger.Verify(l => l.LogInformation("'bin' and 'obj' folders removed successfully!"), Times.Once);
            mockLogger.Verify(l => l.LogInformation("Solution cleaned successfully!"), Times.Once);
            mockCmdHelper.Verify(c => c.RunCmd("dotnet clean", It.IsAny<string>()), Times.Once);
        }
        finally
        {
            // Cleanup
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }
}
