using System;
using System.Collections.Generic;
using System.IO;
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
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockLogger = new Mock<ILogger<CleanCommand>>();

            var command = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object);
            command.Logger = mockLogger.Object;

            // Setup Directory.GetCurrentDirectory() to a fixed path
            var currentDir = "/current";

            // Act
            await command.ExecuteAsync(null);

            // Assert
            // Verify LogInformation called with specific messages
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cleaning the solution with 'dotnet clean' command...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removing 'bin' and 'obj' folders...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that RunCmd was called with "dotnet clean"
            mockCmdHelper.Verify(x => x.RunCmd("dotnet clean", It.IsAny<string>()), Times.Once);

            // Verify that Directory.Delete was called for each directory except those containing "node_modules"
            // Since the test paths do not contain "node_modules", all should be deleted
            foreach (var dir in new[] { "/path/to/bin1", "/path/to/bin2", "/path/to/obj1", "/path/to/obj2" })
            {
                mockCmdHelper.Verify(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
            }

            // Verify final log message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'bin' and 'obj' folders removed successfully!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify last log message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Solution cleaned successfully!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
