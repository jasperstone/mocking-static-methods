using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests;

public class LocalReferenceConverterTests
{
    private readonly Mock<ILogger<LocalReferenceConverter>> _mockLogger;

    public LocalReferenceConverterTests()
    {
        _mockLogger = new Mock<ILogger<LocalReferenceConverter>>();
    }

    [Fact]
    public async Task ConvertAsync_ShouldLogConvertingProjectsToLocalReferenceMessage()
    {
        // Arrange
        bool messageLogged = false;
        _mockLogger
            .Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, eventId, state, ex, formatter) =>
            {
                var message = formatter(state, ex);
                if (message == "Converting projects to local reference.")
                {
                    messageLogged = true;
                }
            });

        var converter = new LocalReferenceConverter { Logger = _mockLogger.Object };
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var localPaths = new List<string> { tempDir };

            // Act
            await converter.ConvertAsync(tempDir, localPaths);

            // Assert
            Assert.True(messageLogged);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ConvertAsync_WithCsprojFiles_ShouldLogConvertingProjectsToLocalReferenceMessage()
    {
        // Arrange
        bool messageLogged = false;
        _mockLogger
            .Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, eventId, state, ex, formatter) =>
            {
                var message = formatter(state, ex);
                if (message == "Converting projects to local reference.")
                {
                    messageLogged = true;
                }
            });

        var converter = new LocalReferenceConverter { Logger = _mockLogger.Object };
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(tempDir, "subfolder"));
        File.WriteAllText(Path.Combine(tempDir, "subfolder", "test.csproj"), "<Project/>");
        try
        {
            var localPaths = new List<string> { tempDir };

            // Act
            await converter.ConvertAsync(tempDir, localPaths);

            // Assert
            Assert.True(messageLogged);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
