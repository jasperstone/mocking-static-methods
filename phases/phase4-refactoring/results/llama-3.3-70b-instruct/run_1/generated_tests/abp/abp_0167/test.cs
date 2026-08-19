using Xunit;
using Moq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class CleanCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CleanCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var cleanCommand = new CleanCommand(cmdHelperMock.Object, telemetryServiceMock.Object);
            cleanCommand.Logger = loggerMock.Object;

            // Act
            await cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_RemovesBinAndObjFolders()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CleanCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var cleanCommand = new CleanCommand(cmdHelperMock.Object, telemetryServiceMock.Object);
            cleanCommand.Logger = loggerMock.Object;
            var binFolder = Path.Combine(Directory.GetCurrentDirectory(), "bin");
            var objFolder = Path.Combine(Directory.GetCurrentDirectory(), "obj");
            Directory.CreateDirectory(binFolder);
            Directory.CreateDirectory(objFolder);

            // Act
            await cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            Assert.False(Directory.Exists(binFolder));
            Assert.False(Directory.Exists(objFolder));
        }
    }
}
