using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests;

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
    public async Task ConvertAsync_ShouldLogInformation_AtStart()
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
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    func(null!, null!)!.Contains("Converting projects to local reference."))),
            Times.Once);
    }

    [Fact]
    public async Task ConvertAsync_WithTargetProjects_ShouldLogInformation_ForEachProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "test.csproj"), "<Project></Project>");
        var localPaths = new List<string> { "." };

        try
        {
            // Act
            await _converter.ConvertAsync(tempDir, localPaths);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                        func(null!, null!)!.Contains("Converting to local reference:"))),
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
    public async Task ConvertAsync_WithTargetProjects_ShouldLogSummaryInformation()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "test1.csproj"), "<Project></Project>");
        File.WriteAllText(Path.Combine(tempDir, "test2.csproj"), "<Project></Project>");
        var localPaths = new List<string> { "." };

        try
        {
            // Act
            await _converter.ConvertAsync(tempDir, localPaths);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                        func(null!, null!)!.Contains("Converted 2 projects"))),
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
