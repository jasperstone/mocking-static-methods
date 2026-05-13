using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class CleanCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogInformationAndRunCommands()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CleanCommand>>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockTelemetryScope = new Mock<IAsyncDisposable>();

            mockTelemetryService
                .Setup(s => s.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(mockTelemetryScope.Object);

            var currentDir = Directory.GetCurrentDirectory();

            // Setup directories to enumerate
            var binDir = Path.Combine(currentDir, "bin");
            var objDir = Path.Combine(currentDir, "obj");
            Directory.CreateDirectory(binDir);
            Directory.CreateDirectory(objDir);

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
                x => x.LogInformation(It.Is<string>(s => s.StartsWith("Deleting: "))),
                Times.Exactly(2));
            mockLogger.Verify(
                x => x.LogInformation($"'bin' and 'obj' folders removed successfully!"),
                Times.Once);
            mockLogger.Verify(
                x => x.LogInformation("Solution cleaned successfully!"),
                Times.Once);

            // Cleanup
            Directory.Delete(binDir, true);
            Directory.Delete(objDir, true);
        }
    }
}
