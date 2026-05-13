using System;
using System.IO;
using System.Linq;
using System.Text;
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
        public async Task ExecuteAsync_Should_LogInformation_CalledOnLine55()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockLogger = new Mock<ILogger<CleanCommand>>();

            var command = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object);
            command.Logger = mockLogger.Object;

            // Setup Directory.EnumerateDirectories to return some dummy paths
            var binDirs = new[] { "bin1", "bin2" }.Select(d => Path.Combine(Directory.GetCurrentDirectory(), d));
            var objDirs = new[] { "obj1", "obj2" }.Select(d => Path.Combine(Directory.GetCurrentDirectory(), d));

            // Mock Directory.EnumerateDirectories
            var directoryMock = new Mock<IDirectoryWrapper>();
            directoryMock.Setup(d => d.EnumerateDirectories(It.IsAny<string>(), "bin", SearchOption.AllDirectories))
                .Returns(binDirs);
            directoryMock.Setup(d => d.EnumerateDirectories(It.IsAny<string>(), "obj", SearchOption.AllDirectories))
                .Returns(objDirs);

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Cleaning the solution with 'dotnet clean' command..."),
                Times.Once);
        }
    }
}
