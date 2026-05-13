using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

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
            var cleanCommand = new CleanCommand(cmdHelperMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_CallsRunCmd()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CleanCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var cleanCommand = new CleanCommand(cmdHelperMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await cleanCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            cmdHelperMock.Verify(c => c.RunCmd(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RemovesBinAndObjFolders()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CleanCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var cleanCommand = new CleanCommand(cmdHelperMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Create test bin and obj folders
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
