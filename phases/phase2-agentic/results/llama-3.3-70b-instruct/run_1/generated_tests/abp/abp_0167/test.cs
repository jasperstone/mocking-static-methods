using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Core.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Commands
{
    public class CleanCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CleanCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var cleanCommand = new CleanCommand(cmdHelperMock.Object, telemetryServiceMock.Object);
            cleanCommand.Logger = loggerMock.Object;

            // Act
            await cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Cleaning the solution with 'dotnet clean' command..."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation($"Removing 'bin' and 'obj' folders..."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation($"'bin' and 'obj' folders removed successfully!"), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Solution cleaned successfully!"), Times.Once);
        }
    }
}
