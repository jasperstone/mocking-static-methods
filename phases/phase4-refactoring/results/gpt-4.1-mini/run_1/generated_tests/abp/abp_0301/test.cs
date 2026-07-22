using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_TranslateOnline_LogsWriteTranslationJsonMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var options = new Dictionary<string, string>
            {
                { "--culture", "fr" },
                { "--online", "" },
                { "--deepl-auth-key", "fake-auth-key" },
                { "--reference-culture", "en" }
            };
            var args = new CommandLineArgs();
            foreach (var kvp in options)
            {
                args.Options[kvp.Key] = kvp.Value;
            }

            // We need to create dummy files and directories for the test to avoid exceptions
            var currentDirectory = Directory.GetCurrentDirectory();
            var enJsonPath = Path.Combine(currentDirectory, "en.json");
            var frJsonPath = Path.Combine(currentDirectory, "fr.json");

            // Create dummy en.json file with minimal valid content
            File.WriteAllText(enJsonPath, "{\"texts\":[{\"name\":\"Hello\",\"value\":\"Hello\"}]}");
            // Create dummy fr.json file with minimal valid content
            File.WriteAllText(frJsonPath, "{\"texts\":[{\"name\":\"Hello\",\"value\":\"Bonjour\"}]}");

            try
            {
                // Act
                await command.ExecuteAsync(args);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Write translation json to")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                // Cleanup
                if (File.Exists(enJsonPath)) File.Delete(enJsonPath);
                if (File.Exists(frJsonPath)) File.Delete(frJsonPath);
            }
        }
    }
}
