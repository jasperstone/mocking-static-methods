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
        public async Task ExecuteAsync_LogsInformationAndDeletesDirectories()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockLogger = new Mock<ILogger<CleanCommand>>();
            var mockActivity = new Mock<IAsyncDisposable>();

            mockTelemetryService
                .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(mockActivity.Object);

            // Setup directories to simulate bin and obj folders
            var currentDir = Directory.GetCurrentDirectory();

            var binDirs = new List<string>
            {
                Path.Combine(currentDir, "project1", "bin"),
                Path.Combine(currentDir, "project2", "bin")
            };
            var objDirs = new List<string>
            {
                Path.Combine(currentDir, "project1", "obj"),
                Path.Combine(currentDir, "project2", "obj")
            };

            // Create the directories for the test
            foreach (var dir in binDirs.Concat(objDirs))
            {
                Directory.CreateDirectory(dir);
            }

            // Add a node_modules directory to test skipping
            var nodeModulesDir = Path.Combine(currentDir, "project1", "node_modules");
            Directory.CreateDirectory(nodeModulesDir);

            // We will override Directory.EnumerateDirectories by using a shim method in CleanCommand
            // but since we cannot override static methods, we will simulate by creating the directories
            // and rely on the actual Directory.EnumerateDirectories call.

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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cleaning the solution with 'dotnet clean' command...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockCmdHelper.Verify(c => c.RunCmd("dotnet clean", It.IsAny<string>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removing 'bin' and 'obj' folders...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            // Verify skipping node_modules log
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Skipping:") && v.ToString().Contains("node_modules")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            // Verify deleting logs for bin and obj folders
            foreach (var path in binDirs.Concat(objDirs))
            {
                mockLogger.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting:") && v.ToString().Contains(path)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

                // The directory should be deleted
                Assert.False(Directory.Exists(path));
            }

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'bin' and 'obj' folders removed successfully!")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Solution cleaned successfully!")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            // Cleanup node_modules directory
            if (Directory.Exists(nodeModulesDir))
            {
                Directory.Delete(nodeModulesDir, true);
            }
        }
    }
}
