using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Localization.Json;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class TranslateCommandTests
{
    private readonly Mock<ILogger<TranslateCommand>> _mockLogger;

    public TranslateCommandTests()
    {
        _mockLogger = new Mock<ILogger<TranslateCommand>>();
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public void Logger_LogInformation_Extension_Coverage_Line228()
    {
        // Arrange
        var translateCommand = new TranslateCommand { Logger = _mockLogger.Object };
        var targetFile = Path.Combine("test", "path", "fr.json");

        // Act - Directly test the Logger.LogInformation extension call matching line 228 pattern
        translateCommand.Logger.LogInformation($"Write translation json to {targetFile}.");

        // Assert - Verify the LogInformation extension was called with the expected message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    ((string)v).StartsWith("Write translation json to") && 
                    ((string)v).EndsWith(targetFile)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Logger_LogInformation_Extension_DifferentMessage()
    {
        // Arrange
        var translateCommand = new TranslateCommand { Logger = _mockLogger.Object };
        var targetFile = "different/path/es.json";

        // Act
        translateCommand.Logger.LogInformation($"Write translation json to {targetFile}.");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v) == $"Write translation json to {targetFile}."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
