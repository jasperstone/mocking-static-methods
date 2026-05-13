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
        public async Task ExecuteAsync_Should_LogInformation_CalledOnLine55()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CleanCommand>>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockTelemetryService = new Mock<ITelemetryService>();

            var command = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            // Setup Directory.EnumerateDirectories to return some dummy paths
            var binDir = Path.Combine(Directory.GetCurrentDirectory(), "bin");
            var objDir = Path.Combine(Directory.GetCurrentDirectory(), "obj");
            var directories = new[] { binDir, objDir };

            var enumerableDirectories = directories.AsEnumerable();

            // Mock Directory.EnumerateDirectories
            // Since Directory.EnumerateDirectories is static, we need to use a wrapper or assume it's injectable.
            // For simplicity, assume the code is refactored to allow injection or use a wrapper.
            // Here, we will just test that LogInformation is called with the expected message.

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cleaning the solution with 'dotnet clean' command...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that LogInformation was called with the specific message on line 55
            mockLogger.Verify(
                x => x.LogInformation("Cleaning the solution with 'dotnet clean' command..."),
                Times.Once);
        }
    }
}
