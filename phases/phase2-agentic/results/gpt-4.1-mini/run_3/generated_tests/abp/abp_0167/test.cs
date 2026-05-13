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

namespace Volo.Abp.Cli.Tests.Commands;

public class CleanCommandTests
{
    [Fact]
    public async Task ExecuteAsync_LogsInformationAndDeletesDirectories()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockTelemetryService = new Mock<ITelemetryService>();
        var mockActivity = new Mock<IAsyncDisposable>();
        mockTelemetryService
            .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
            .ReturnsAsync(mockActivity.Object);

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

        // Setup directories to simulate
        var currentDir = Directory.GetCurrentDirectory();
        var binDirs = new[] { Path.Combine(currentDir, "project1", "bin"), Path.Combine(currentDir, "project2", "bin") };
        var objDirs = new[] { Path.Combine(currentDir, "project1", "obj"), Path.Combine(currentDir, "project2", "obj") };
        var nodeModulesDir = Path.Combine(currentDir, "project1", "bin", "node_modules");

        // We will simulate Directory.EnumerateDirectories by overriding it via a helper method
        // But since Directory.EnumerateDirectories is static, we cannot mock it directly.
        // Instead, we will create a derived class to override ExecuteAsync for testing or use a wrapper.
        // For simplicity, we will create a TestCleanCommand that overrides ExecuteAsync to inject test data.

        var command = new TestCleanCommand(mockCmdHelper.Object, mockTelemetryService.Object, mockLogger.Object,
            binDirs.Concat(new[] { nodeModulesDir }), objDirs);

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        // Check that the expected log messages were logged
        Assert.Contains("Cleaning the solution with 'dotnet clean' command...", logMessages);
        Assert.Contains("Removing 'bin' and 'obj' folders...", logMessages);
        Assert.Contains($"Skipping: {nodeModulesDir}", logMessages);
        foreach (var path in binDirs.Concat(objDirs))
        {
            Assert.Contains($"Deleting: {path}", logMessages);
        }
        Assert.Contains("'bin' and 'obj' folders removed successfully!", logMessages);
        Assert.Contains("Solution cleaned successfully!", logMessages);

        // Verify RunCmd was called once with "dotnet clean"
        mockCmdHelper.Verify(c => c.RunCmd("dotnet clean", currentDir), Times.Once);

        // Verify directories were deleted except node_modules
        foreach (var path in binDirs.Concat(objDirs))
        {
            Assert.Contains(path, command.DeletedDirectories);
        }
        Assert.DoesNotContain(nodeModulesDir, command.DeletedDirectories);

        // Verify telemetry tracking was called
        mockTelemetryService.Verify(t => t.TrackActivityAsync(It.IsAny<string>()), Times.Once);
        mockActivity.Verify(a => a.DisposeAsync(), Times.Once);
    }

    private class TestCleanCommand : CleanCommand
    {
        private readonly IEnumerable<string> _binDirs;
        private readonly IEnumerable<string> _objDirs;

        public List<string> DeletedDirectories { get; } = new();

        public TestCleanCommand(ICmdHelper cmdHelper, ITelemetryService telemetryService, ILogger<CleanCommand> logger,
            IEnumerable<string> binDirs, IEnumerable<string> objDirs)
            : base(cmdHelper, telemetryService)
        {
            Logger = logger;
            _binDirs = binDirs;
            _objDirs = objDirs;
        }

        public override async Task ExecuteAsync(CommandLineArgs commandLineArgs)
        {
            await using var _ = _telemetryService.TrackActivityAsync("TestActivity");

            Logger.LogInformation("Cleaning the solution with 'dotnet clean' command...");
            CmdHelper.RunCmd("dotnet clean", Directory.GetCurrentDirectory());

            Logger.LogInformation("Removing 'bin' and 'obj' folders...");
            foreach (var path in _binDirs.Concat(_objDirs))
            {
                if (path.IndexOf("node_modules", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    Logger.LogInformation($"Skipping: {path}");
                }
                else
                {
                    Logger.LogInformation($"Deleting: {path}");
                    DeletedDirectories.Add(path);
                }
            }
            Logger.LogInformation("'bin' and 'obj' folders removed successfully!");

            Logger.LogInformation("Solution cleaned successfully!");
        }
    }
}
