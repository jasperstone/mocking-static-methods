using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task TranslateCommand_LogsInformationOnWriteTranslationJson()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "culture", "fr" },
                    { "referenceCulture", "en" },
                    { "apply", "true" },
                    { "file", "abp-translation.json" }
                }
            };

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Write translation json to")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
