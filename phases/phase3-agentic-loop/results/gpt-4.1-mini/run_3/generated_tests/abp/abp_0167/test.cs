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
            var mockLogger = new Mock<ILogger<CleanCommand>>();

            // Setup telemetry to return a disposable that does nothing
            mockTelemetryService
                .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(new DummyAsyncDisposable());

            // Setup CmdHelper.RunCmd to do nothing
            mockCmdHelper
                .Setup(c => c.RunCmd(It.IsAny<string>(), It.IsAny<string>()));

            // Setup directory enumeration to simulate bin and obj folders
            var currentDir = Directory.GetCurrentDirectory();
            var binDirs = new List<string> { Path.Combine(currentDir, "project1", "bin") };
            var objDirs = new List<string> { Path.Combine(currentDir, "project1", "obj") };

            // We will override Directory.EnumerateDirectories by using a shim via a helper class
            // But since we cannot override static methods easily, we will create a derived class for testing
            // Instead, we will temporarily create the directories on disk and delete after test
            // But to avoid side effects, we will mock Directory.Delete and Directory.EnumerateDirectories using a wrapper interface
            // Since the code uses static Directory methods directly, we cannot mock them easily here.
            // So we will create the directories and delete them after test.

            // Create test directories
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
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Cleaning the solution with 'dotnet clean' command..."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Removing 'bin' and 'obj' folders..."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that the "Deleting: ..." log was called for each bin and obj folder
            foreach (var path in binDirs.Concat(objDirs))
            {
                mockLogger.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Deleting: {path}"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            // Verify the line 55 log: "'bin' and 'obj' folders removed successfully!"
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "'bin' and 'obj' folders removed successfully!"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify final success log
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Solution cleaned successfully!"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify CmdHelper.RunCmd was called with "dotnet clean"
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

        private class DummyAsyncDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}
