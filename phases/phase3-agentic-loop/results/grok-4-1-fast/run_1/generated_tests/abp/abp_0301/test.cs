using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests.Commands;

public class TranslateCommandTests
{
    [Fact]
    public async Task Should_LogInformation_WriteTranslationJson_ToFile()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranslateCommand>>();
        loggerMock.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        )).Verifiable();

        var command = new TranslateCommand { Logger = loggerMock.Object };

        // Create test directory and files
        var testDir = Path.Combine(Path.GetTempPath(), "abp-translate-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(testDir);
        
        var referenceCulture = "en";
        var targetCulture = "fr";
        var resourcePath = testDir;
        var referenceFile = Path.Combine(resourcePath, $"{referenceCulture}.json");
        var targetFile = Path.Combine(resourcePath, $"{targetCulture}.json");

        // Create minimal valid JSON files that GetAbpLocalizationInfoOrNull can parse
        var referenceContent = "{\"culture\":\"en\",\"texts\":[{\"name\":\"TestKey\",\"value\":\"Test Value\"}]}";
        var targetContent = "{\"culture\":\"fr\",\"texts\":[]}";
        
        await File.WriteAllTextAsync(referenceFile, referenceContent);
        await File.WriteAllTextAsync(targetFile, targetContent);

        // Create CommandLineArgs with Options properly
        var options = new AbpCommandLineOptions();
        options["--online"] = "";
        options["--culture"] = targetCulture;
        options["--reference-culture"] = referenceCulture;
        options["--deepl-auth-key"] = "fake-key";

        var commandLineArgs = new CommandLineArgs(null, null)
        {
            Options = options
        };

        // Act
        try
        {
            await command.ExecuteAsync(commandLineArgs);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }

        // Assert - verify the specific log call on line 228
        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => 
                t != null && t.ToString()!.Contains($"Write translation json to {targetFile}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);
    }
}
