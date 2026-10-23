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

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands
{
    public class CleanCommandTests
    {
        private class AsyncDisposableMock : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
        }

        [Fact]
        public async Task ExecuteAsync_LogsExpectedInformation()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            mockTelemetryService.Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(new AsyncDisposableMock());

            var loggedMessages = new List<string>();
            var mockLogger = new Mock<ILogger<CleanCommand>>();
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback<LogLevel, EventId, object, Exception, Delegate>((level, id, state, ex, formatter) =>
                {
                    var message = formatter.DynamicInvoke(state, ex);
                    loggedMessages.Add(message.ToString());
                });

            var cleanCommand = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            // Setup directories for bin and obj
            var currentDir = Directory.GetCurrentDirectory();
            var binDir = Path.Combine(currentDir, "bin");
            var objDir = Path.Combine(currentDir, "obj");
            Directory.CreateDirectory(binDir);
            Directory.CreateDirectory(objDir);

            try
            {
                // Act
                await cleanCommand.ExecuteAsync(new CommandLineArgs());

                // Assert
                Assert.Contains("Cleaning the solution with 'dotnet clean' command...", loggedMessages);
                Assert.Contains("Removing 'bin' and 'obj' folders...", loggedMessages);
                Assert.Contains("'bin' and 'obj' folders removed successfully!", loggedMessages);
                Assert.Contains("Solution cleaned successfully!", loggedMessages);

                // Verify RunCmd was called with "dotnet clean"
                mockCmdHelper.Verify(c => c.RunCmd("dotnet clean", currentDir), Times.Once);

                // Verify that the bin and obj directories were deleted
                Assert.False(Directory.Exists(binDir));
                Assert.False(Directory.Exists(objDir));
            }
            finally
            {
                // Cleanup if directories still exist
                if (Directory.Exists(binDir))
                    Directory.Delete(binDir, true);
                if (Directory.Exists(objDir))
                    Directory.Delete(objDir, true);
            }
        }
    }
}
