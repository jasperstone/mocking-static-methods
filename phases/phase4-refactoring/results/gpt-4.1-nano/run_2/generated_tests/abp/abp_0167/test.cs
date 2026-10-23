using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class CleanCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogInformationAndRunCommands()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var loggerMock = new Mock<ILogger<CleanCommand>>();

            var command = new CleanCommand(cmdHelperMock.Object, telemetryServiceMock.Object);
            command.Logger = loggerMock.Object;

            // Setup Directory.EnumerateDirectories to return test paths
            var binDirs = new[] { "/path/to/bin1", "/path/to/bin2" };
            var objDirs = new[] { "/path/to/obj1" };
            var allDirs = binDirs.Concat(objDirs).ToArray();

            // Mock Directory static methods
            var directoryMock = new Mock<IDirectoryWrapper>();
            directoryMock.Setup(d => d.EnumerateDirectories(It.IsAny<string>(), "bin", SearchOption.AllDirectories))
                .Returns(binDirs);
            directoryMock.Setup(d => d.EnumerateDirectories(It.IsAny<string>(), "obj", SearchOption.AllDirectories))
                .Returns(objDirs);

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(x => x.LogInformation("Cleaning the solution with 'dotnet clean' command..."), Times.Once);
            loggerMock.Verify(x => x.LogInformation($"Removing 'bin' and 'obj' folders..."), Times.Once);
            foreach (var dir in allDirs)
            {
                loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.StartsWith("Deleting:"))), Times.Exactly(allDirs.Length));
            }
            loggerMock.Verify(x => x.LogInformation($"'bin' and 'obj' folders removed successfully!"), Times.Once);
            loggerMock.Verify(x => x.LogInformation("Solution cleaned successfully!"), Times.Once);
            cmdHelperMock.Verify(x => x.RunCmd("dotnet clean", It.IsAny<string>()), Times.Once);
        }
    }
}
