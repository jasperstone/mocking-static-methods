using Moq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class CleanCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationAfterRemovingFolders()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CleanCommand>>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockTelemetryService = new Mock<ITelemetryService>();

            var cleanCommand = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            // Simulate directory structure
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "bin"));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "obj"));

            // Act
            await cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(It.Is<string>(s => s.Contains("'bin' and 'obj' folders removed successfully!"))),
                Times.Once);
        }
    }
}
