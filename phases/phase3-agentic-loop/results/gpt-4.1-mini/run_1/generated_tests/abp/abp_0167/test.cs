using System;
using System.Collections.Generic;
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

namespace Volo.Abp.Cli.Tests.Commands
{
    public class CleanCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationIncludingLine55()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockActivity = new Mock<IAsyncDisposable>();
            mockTelemetryService
                .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(mockActivity.Object));

            var logMessages = new List<string>();
            var mockLogger = new Mock<ILogger<CleanCommand>>();
            mockLogger
                .Setup(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback<LogLevel, EventId, object, Exception, Delegate>((level, id, state, ex, formatter) =>
                {
                    var message = formatter.DynamicInvoke(state, ex) as string;
                    logMessages.Add(message);
                });

            // Setup directories to simulate bin and obj folders
            var currentDir = Directory.GetCurrentDirectory();
            var binDirs = new[] { Path.Combine(currentDir, "project1", "bin"), Path.Combine(currentDir, "project2", "bin") };
            var objDirs = new[] { Path.Combine(currentDir, "project1", "obj"), Path.Combine(currentDir, "project2", "obj") };

            // Create temp directories for bin and obj
            foreach (var dir in binDirs.Concat(objDirs))
            {
                Directory.CreateDirectory(dir);
            }

            var cleanCommand = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            // Check that the specific log message on line 55 is logged
            Assert.Contains("'bin' and 'obj' folders removed successfully!", logMessages);

            // Check some other expected log messages
            Assert.Contains("Cleaning the solution with 'dotnet clean' command...", logMessages);
            Assert.Contains("Removing 'bin' and 'obj' folders...", logMessages);
            Assert.Contains("Deleting: " + binDirs[0], logMessages);
            Assert.Contains("Deleting: " + objDirs[0], logMessages);
            Assert.Contains("Solution cleaned successfully!", logMessages);

            // Verify that RunCmd was called with "dotnet clean"
            mockCmdHelper.Verify(c => c.RunCmd("dotnet clean", It.IsAny<string>()), Times.Once);

            // Cleanup created directories
            foreach (var dir in binDirs.Concat(objDirs))
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }
    }
}
