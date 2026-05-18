using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Localization;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task TranslateCommand_ExecuteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };
            var commandLineArgs = new CommandLineArgs();

            // Act
            await translateCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TranslateCommand_TranslateAbpTranslateInfoAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };
            var directory = Directory.GetCurrentDirectory();
            var targetCulture = "en";
            var referenceCulture = "en";
            var allValues = false;
            var authKey = "authKey";

            // Act
            await translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TranslateCommand_GenerateAbpTranslateInfoAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };
            var directory = Directory.GetCurrentDirectory();
            var targetCulture = "en";
            var referenceCulture = "en";
            var allValues = false;
            var outputFile = "outputFile.json";

            // Act
            await translateCommand.GenerateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, outputFile);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
