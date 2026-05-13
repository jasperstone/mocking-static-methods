using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

public class CleanCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformationMessages()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockLogger = new Mock<ILogger<CleanCommand>>();
        var command = new CleanCommand(mockCmdHelper.Object, null)
        {
            Logger = mockLogger.Object
        };

        var commandLineArgs = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>()),
            Times.Exactly(5));

        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>()),
            Times.Once(),
            "Cleaning the solution with 'dotnet clean' command...");

        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>()),
            Times.Once(),
            "Removing 'bin' and 'obj' folders...");

        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>()),
            Times.Once(),
            "'bin' and 'obj' folders removed successfully!");

        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>()),
            Times.Once(),
            "Solution cleaned successfully!");
    }
}
