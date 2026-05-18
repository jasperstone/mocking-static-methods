using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class TranslateCommandTests
{
    [Fact]
    public async Task Should_Log_WriteTranslationJsonMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranslateCommand>>();
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        var command = new TranslateCommand { Logger = loggerMock.Object };

        var currentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(currentDirectory);
        
        try
        {
            // Create directory structure
            var resourceDir = Path.Combine(currentDirectory, "Localization");
            Directory.CreateDirectory(resourceDir);
            
            // Create reference file
            var referenceFilePath = Path.Combine(resourceDir, "en.json");
            File.WriteAllText(referenceFilePath, 
                "{\"Culture\":\"en\",\"Texts\":[{\"Name\":\"TestKey\",\"Value\":\"Test Value\"}]}");

            var options = new AbpCommandLineOptions();
            options["--online"] = "";
            options["--culture"] = "tr";
            options["--deepL-auth-key"] = "test-key";

            var commandLineArgs = new CommandLineArgs(null, null);
            commandLineArgs.Options = options;

            // Set current directory
            var prevDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(currentDirectory);
            
            try
            {
                // Act
                await command.ExecuteAsync(commandLineArgs);
            }
            finally
            {
                Directory.SetCurrentDirectory(prevDir);
            }
        }
        finally
        {
            if (Directory.Exists(currentDirectory))
            {
                Directory.Delete(currentDirectory, true);
            }
        }

        // Assert - Verify the specific log message for line 228
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Write translation json to")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());
    }
}
