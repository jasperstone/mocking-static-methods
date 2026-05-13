using System;
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

namespace Volo.Abp.Cli.Core.Tests.Commands
{
    public class CleanCommandTests
    {
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<ITelemetryService> _telemetryServiceMock;
        private readonly Mock<ILogger<CleanCommand>> _loggerMock;
        private readonly CleanCommand _cleanCommand;

        public CleanCommandTests()
        {
            _cmdHelperMock = new Mock<ICmdHelper>();
            _telemetryServiceMock = new Mock<ITelemetryService>();
            _loggerMock = new Mock<ILogger<CleanCommand>>();
            _cleanCommand = new CleanCommand(_cmdHelperMock.Object, _telemetryServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformationMessages()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("clean");
            var currentDirectory = Directory.GetCurrentDirectory();
            var binEntries = new[] { Path.Combine(currentDirectory, "bin") };
            var objEntries = new[] { Path.Combine(currentDirectory, "obj") };

            _telemetryServiceMock.Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new DisposableActivity()));

            _cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), It.IsAny<string>()));

            Directory.SetupDirectoryEntries(currentDirectory, binEntries, objEntries);

            // Act
            await _cleanCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Cleaning the solution with 'dotnet clean' command..."), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("Removing 'bin' and 'obj' folders..."), Times.Once);
            _loggerMock.Verify(x => x.LogInformation($"Deleting: {binEntries[0]}"), Times.Once);
            _loggerMock.Verify(x => x.LogInformation($"Deleting: {objEntries[0]}"), Times.Once);
            _loggerMock.Verify(x => x.LogInformation($"'bin' and 'obj' folders removed successfully!"), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("Solution cleaned successfully!"), Times.Once);
        }

        private static class Directory
        {
            public static void SetupDirectoryEntries(string currentDirectory, string[] binEntries, string[] objEntries)
            {
                var directoryInfoMock = new Mock<DirectoryInfo>(currentDirectory);
                directoryInfoMock.Setup(x => x.EnumerateDirectories("bin", SearchOption.AllDirectories))
                    .Returns(binEntries.Select(x => new DirectoryInfo(x)));
                directoryInfoMock.Setup(x => x.EnumerateDirectories("obj", SearchOption.AllDirectories))
                    .Returns(objEntries.Select(x => new DirectoryInfo(x)));
            }
        }

        private class DisposableActivity : IAsyncDisposable
        {
            public ValueTask DisposeAsync()
            {
                return new ValueTask(Task.CompletedTask);
            }
        }
    }
}
