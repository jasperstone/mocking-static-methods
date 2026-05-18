using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

public class GenerateRazorPageTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenFilesGenerated()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
        var command = new GenerateRazorPage
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
