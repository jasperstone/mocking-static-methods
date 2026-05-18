using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Localization.Json;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogInformation_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var directory = Directory.GetCurrentDirectory();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = false;
            var authKey = "authKey";

            var args = new CommandLineArgs("--online --culture " + targetCulture + " --reference-culture " + referenceCulture + " --deepl-auth-key " + authKey);

            // Act
            await translateCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogInformation_WriteTranslationJsonToTargetFile_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var directory = Directory.GetCurrentDirectory();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = false;
            var authKey = "authKey";

            var args = new CommandLineArgs("--online --culture " + targetCulture + " --reference-culture " + referenceCulture + " --deepl-auth-key " + authKey);

            // Act
            await translateCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation($"Write translation json to {Path.Combine(directory, $"{targetCulture}.json")}."));
        }
    }
}
