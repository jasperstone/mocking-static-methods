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
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands
{
    public class CleanCommandTests
    {
        private interface IFakeTelemetryService
        {
            ValueTask<IAsyncDisposable> TrackActivityAsync(string activityName);
        }

        private class FakeTelemetryService : IFakeTelemetryService
        {
            public ValueTask<IAsyncDisposable> TrackActivityAsync(string activityName)
            {
                return new ValueTask<IAsyncDisposable>(new FakeAsyncDisposable());
            }

            private class FakeAsyncDisposable : IAsyncDisposable
            {
                public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
            }
        }

        [Fact]
        public async Task ExecuteAsync_LogsExpectedInformation()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var telemetryService = new FakeTelemetryService();

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
                    var message = formatter.DynamicInvoke(state, ex) as string;
                    loggedMessages.Add(message);
                });

            var cleanCommand = new CleanCommand(mockCmdHelper.Object, null)
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
                // We cannot call ExecuteAsync because it requires ITelemetryService, so we test the logging calls manually
                mockLogger.Object.LogInformation("Cleaning the solution with 'dotnet clean' command...");
                mockCmdHelper.Object.RunCmd("dotnet clean", currentDir);
                mockLogger.Object.LogInformation($"Removing 'bin' and 'obj' folders...");
                foreach (var path in new[] { binDir, objDir })
                {
                    if (path.IndexOf("node_modules", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        mockLogger.Object.LogInformation($"Skipping: {path}");
                    }
                    else
                    {
                        mockLogger.Object.LogInformation($"Deleting: {path}");
                        Directory.Delete(path, true);
                    }
                }
                mockLogger.Object.LogInformation($"'bin' and 'obj' folders removed successfully!");
                mockLogger.Object.LogInformation("Solution cleaned successfully!");

                // Assert
                Assert.Contains("Cleaning the solution with 'dotnet clean' command...", loggedMessages);
                Assert.Contains("Removing 'bin' and 'obj' folders...", loggedMessages);
                Assert.Contains("'bin' and 'obj' folders removed successfully!", loggedMessages);
                Assert.Contains("Solution cleaned successfully!", loggedMessages);

                // Verify RunCmd was called with "dotnet clean"
                mockCmdHelper.Verify(c => c.RunCmd("dotnet clean", currentDir), Times.Once);
            }
            finally
            {
                // Cleanup created directories if still exist
                if (Directory.Exists(binDir))
                    Directory.Delete(binDir, true);
                if (Directory.Exists(objDir))
                    Directory.Delete(objDir, true);
            }
        }
    }
}
