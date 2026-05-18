using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectModification;

public class LocalReferenceConverterTests
{
    private readonly Mock<ILogger<LocalReferenceConverter>> _mockLogger;
    private readonly LocalReferenceConverter _converter;

    public LocalReferenceConverterTests()
    {
        _mockLogger = new Mock<ILogger<LocalReferenceConverter>>();
        _converter = new LocalReferenceConverter
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async void ConvertAsync_Should_LogInformation_ConvertingProjectsToLocalReference()
    {
        // Arrange
        var directory = Directory.GetCurrentDirectory();
        var localPaths = new List<string> { "." };

        // Act
        await _converter.ConvertAsync(directory, localPaths);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async void ConvertAsync_WithEmptyDirectory_Should_LogInformationMessage()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var localPaths = new List<string> { "." };

            // Act
            await _converter.ConvertAsync(tempDir, localPaths);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
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
