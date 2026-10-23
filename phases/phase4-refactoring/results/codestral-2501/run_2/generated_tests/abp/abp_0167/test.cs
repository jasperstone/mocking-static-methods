using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Internal.Telemetry;
using Xunit;

public class CleanCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformationMessages()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockTelemetryService = new Mock<ITelemetryService>();
        var mockLogger = new Mock<ILogger<CleanCommand>>();

        var command = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object)
        {
            Logger = mockLogger.Object
        };

        var commandLineArgs = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(commandLineArgs);

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
}
