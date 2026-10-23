using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Core;
using Volo.Abp.Localization.Json;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class TranslateCommandTests
{
    private readonly Mock<ILogger<TranslateCommand>> _mockLogger;
    private readonly TranslateCommand _translateCommand;

    public TranslateCommandTests()
    {
        _mockLogger = new Mock<ILogger<TranslateCommand>>();
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<object>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<object, Exception?, string>>()));
        _translateCommand = new TranslateCommand
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task Should_Log_WriteTranslationJsonMessage()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var originalDir = Directory.GetCurrentDirectory();
        
        try
        {
            Directory.CreateDirectory(tempDir);
            var resourceDir = Path.Combine(tempDir, "Localization");
            Directory.CreateDirectory(resourceDir);
            
            // Create reference file
            var referenceFilePath = Path.Combine(resourceDir, "en.json");
            File.WriteAllText(referenceFilePath, 
                "{\"Culture\":\"en\",\"Texts\":[{\"Name\":\"TestKey\",\"Value\":\"Test Value\"}]}");
            
            Directory.SetCurrentDirectory(tempDir);

            var options = new AbpCommandLineOptions();
            options.Add("--culture", "tr");
            options.Add("--online", "");
            options.Add("--deepl-auth-key", "fake-key");

            var commandLineArgs = new CommandLineArgs(null, null) { Options = options };

            // Act - expect exception due to fake DeepL key, but logging should occur before translation
            await Assert.ThrowsAsync<CliUsageException>(
                () => _translateCommand.ExecuteAsync(commandLineArgs));
        }
        finally
        {
            try
            {
                Directory.SetCurrentDirectory(originalDir);
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch { }
        }

        // Assert - verify the specific log message from line 228
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(state => state.ToString().Contains("Write translation json")),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
