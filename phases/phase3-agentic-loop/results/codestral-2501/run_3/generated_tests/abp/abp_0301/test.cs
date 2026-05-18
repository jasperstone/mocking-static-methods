using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Localization.Json;
using Xunit;

public class TranslateCommandTests
{
    [Fact]
    public async Task TranslateAbpTranslateInfoAsync_ShouldLogInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TranslateCommand>>();
        var command = new TranslateCommand
        {
            Logger = mockLogger.Object
        };

        var options = new AbpCommandLineOptions
        {
            { "culture", "fr" },
            { "online", "" },
            { "deepl-auth-key", "authKey" }
        };

        var commandLineArgs = new CommandLineArgs("translate", null, options);

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Write translation json to")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
