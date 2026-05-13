using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
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

            // Setup directories to be returned by Directory.EnumerateDirectories
            var currentDir = Directory.GetCurrentDirectory();

            var binDirs = new List<string>
            {
                Path.Combine(currentDir, "project1", "bin"),
                Path.Combine(currentDir, "project2", "bin")
            };
            var objDirs = new List<string>
            {
                Path.Combine(currentDir, "project1", "obj"),
                Path.Combine(currentDir, "project2", "obj"),
                Path.Combine(currentDir, "project2", "node_modules_obj") // This should be skipped
            };

            // We need to mock Directory.EnumerateDirectories and Directory.Delete
            // Since Directory is static, we cannot mock it directly.
            // Instead, we will use a helper class to override these calls in the test.
            // But since the code uses static Directory calls directly, we will simulate by creating temp directories.

            // Create temp directories for testing
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
            // Verify telemetry tracking was called
            mockTelemetryService.Verify(t => t.TrackActivityAsync(It.Is<string>(s => s == "AbpCliCommandsClean")), Times.Once);
            mockActivity.Verify(a => a.DisposeAsync(), Times.Once);

            // Verify CmdHelper.RunCmd was called with "dotnet clean"
            mockCmdHelper.Verify(c => c.RunCmd("dotnet clean", It.IsAny<string>()), Times.Once);

            // Verify logger calls
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cleaning the solution with 'dotnet clean' command...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removing 'bin' and 'obj' folders...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            // Check that skipping log was called for the node_modules path
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Skipping:") && v.ToString().Contains("node_modules_obj")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            // Check that deleting log was called for other paths
            foreach (var path in binDirs.Concat(objDirs).Where(p => !p.Contains("node_modules", StringComparison.OrdinalIgnoreCase)))
            {
                mockLogger.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting:") && v.ToString().Contains(path)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            }

            // Check final logs
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

            // Cleanup created directories if they still exist
            foreach (var dir in binDirs.Concat(objDirs))
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        [Fact]
        public void GetUsageInfo_ReturnsExpectedString()
        {
            // Arrange
            var cleanCommand = new CleanCommand(Mock.Of<ICmdHelper>(), Mock.Of<ITelemetryService>());

            // Act
            var usageInfo = cleanCommand.GetUsageInfo();

            // Assert
            Assert.Contains("Usage:", usageInfo);
            Assert.Contains("abp clean", usageInfo);
            Assert.Contains("https://abp.io/docs/latest/cli", usageInfo);
        }

        [Fact]
        public void GetShortDescription_ReturnsExpectedString()
        {
            // Act
            var description = CleanCommand.GetShortDescription();

            // Assert
            Assert.Equal("Delete all BIN and OBJ folders in current folder.", description);
        }
    }
}
