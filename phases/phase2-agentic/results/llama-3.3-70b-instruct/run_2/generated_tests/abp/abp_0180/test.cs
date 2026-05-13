using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage();
            generateRazorPage.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs();

            // Act
            await generateRazorPage.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage();
            generateRazorPage.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs();

            // Act
            await generateRazorPage.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.Is<string>(s => s.StartsWith("0 files successfully generated."))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformationWithCorrectMessage_MultipleFiles()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage();
            generateRazorPage.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs();

            // Act
            await generateRazorPage.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.Is<string>(s => s.StartsWith("0 files successfully generated."))), Times.Once);
        }
    }
}
