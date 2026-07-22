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

    public LocalReferenceConverterTests()
    {
        _mockLogger = new Mock<ILogger<LocalReferenceConverter>>();
        _mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Converting projects to local reference.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public async Task ConvertAsync_Should_LogInformation_At_Start()
    {
        // Arrange
        var converter = new LocalReferenceConverter { Logger = _mockLogger.Object };
        var directory = Directory.GetCurrentDirectory();
        var localPaths = new List<string> { "." };

        // Act
        await converter.ConvertAsync(directory, localPaths);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Converting projects to local reference.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ConvertAsync_With_No_Csproj_Files_Should_LogInformation()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var converter = new LocalReferenceConverter { Logger = _mockLogger.Object };
            var localPaths = new List<string> { tempDir };

            // Act
            await converter.ConvertAsync(tempDir, localPaths);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Converting projects to local reference.")),
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

    [Fact]
    public async Task ConvertAsync_With_One_Project_Should_LogInformation()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectDir = Path.Combine(tempDir, "TestProject");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "TestProject.csproj"), "<Project></Project>");

        var converter = new LocalReferenceConverter { Logger = _mockLogger.Object };
        var localPaths = new List<string> { tempDir };

        // Act
        await converter.ConvertAsync(tempDir, localPaths);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Converting projects to local reference.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Converted 1 projects")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
