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
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var currentDirectory = Directory.GetCurrentDirectory();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "authKey";

            var args = new CommandLineArgs();
            args.Options.Add("--online");
            args.Options.Add("--culture", targetCulture);
            args.Options.Add("--reference-culture", referenceCulture);
            args.Options.Add("--all-values");
            args.Options.Add("--deepl-auth-key", authKey);

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogInformation_WriteTranslationJsonToTargetFile_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var currentDirectory = Directory.GetCurrentDirectory();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "authKey";

            var args = new CommandLineArgs();
            args.Options.Add("--online");
            args.Options.Add("--culture", targetCulture);
            args.Options.Add("--reference-culture", referenceCulture);
            args.Options.Add("--all-values");
            args.Options.Add("--deepl-auth-key", authKey);

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation($"Write translation json to {Path.Combine(currentDirectory, $"{targetCulture}.json")}."));
        }
    }
}
