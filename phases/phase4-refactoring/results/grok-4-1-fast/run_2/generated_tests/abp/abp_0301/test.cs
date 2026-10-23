using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class TranslateCommandTests : IDisposable
{
    private readonly Mock<ILogger<TranslateCommand>> _loggerMock;
    private readonly TranslateCommand _translateCommand;
    private readonly string _tempDir;

    public TranslateCommandTests()
    {
        _loggerMock = new Mock<ILogger<TranslateCommand>>();
        _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        _translateCommand = new TranslateCommand { Logger = _loggerMock.Object };
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task Should_Log_WriteTranslationJsonMessage_Line228()
    {
        // Arrange
        Directory.SetCurrentDirectory(_tempDir);

        // Create resource directory and files to reach line 228
        var resourceDir = Path.Combine(_tempDir, "Localization");
        Directory.CreateDirectory(resourceDir);
        
        // Create reference file (en.json)
        var referenceFile = Path.Combine(resourceDir, "en.json");
        var referenceContent = @"{
  ""culture"": ""en"",
  ""texts"": [
    {
      ""name"": ""TestKey"",
      ""value"": ""Test Value""
    }
  ]
}";
        File.WriteAllText(referenceFile, referenceContent);

        // Create target file (fr.json) - empty initially
        var targetFile = Path.Combine(resourceDir, "fr.json");
        var targetContent = @"{
  ""culture"": ""fr"",
  ""texts"": []
}";
        File.WriteAllText(targetFile, targetContent);

        // Create abp-translation.json to mock the translation result
        var translationFile = Path.Combine(_tempDir, "abp-translation.json");
        var translationContent = @"{
  ""referenceCulture"": ""en"",
  ""targetCulture"": ""fr"",
  ""resources"": [
    {
      ""resourcePath"": ""Localization"",
      ""texts"": [
        {
          ""localizationKey"": ""TestKey"",
          ""reference"": ""Test Value"",
          ""target"": ""Valeur de test""
        }
      ]
    }
  ]
}";
        File.WriteAllText(translationFile, translationContent);

        // Command line args for apply mode (bypasses DeepL)
        var args = new CommandLineArgs();
        args.Options["apply"] = "";

        // Act
        await _translateCommand.ExecuteAsync(args);

        // Assert - verify the LogInformation call on line 228
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Write translation json")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
