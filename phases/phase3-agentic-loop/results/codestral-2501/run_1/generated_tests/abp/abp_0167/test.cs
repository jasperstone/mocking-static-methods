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
    public async Task ExecuteAsync_ShouldLogCorrectMessages()
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
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Cleaning the solution with 'dotnet clean' command..."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Removing 'bin' and 'obj' folders..."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.AtLeastOnce);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "'bin' and 'obj' folders removed successfully!"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Solution cleaned successfully!"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }
}
