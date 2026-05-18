using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

public class CleanCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformationCalls()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockTelemetryService = new Mock<ITelemetryService>();
        var mockLogger = new Mock<ILogger<CleanCommand>>();

        var command = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object);
        command.Logger = mockLogger.Object;

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cleaning the solution with 'dotnet clean' command...")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removing 'bin' and 'obj' folders...")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

        // Since directory enumeration is environment-dependent, we assume the test environment has some directories
        // and focus on verifying that the log messages for deleting are called at least once
        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting:")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'bin' and 'obj' folders removed successfully!")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Solution cleaned successfully!")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
