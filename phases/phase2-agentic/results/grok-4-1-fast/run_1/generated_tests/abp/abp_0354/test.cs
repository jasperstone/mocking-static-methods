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
    private readonly Mock<ILogger<LocalReferenceConverter>> _loggerMock;
    private readonly LocalReferenceConverter _converter;

    public LocalReferenceConverterTests()
    {
        _loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
        _converter = new LocalReferenceConverter
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async void ConvertAsync_ShouldLogInformation_WhenCalled()
    {
        // Arrange
        var directory = "/test/directory";
        var localPaths = new List<string> { "/test/local" };

        // Mock static methods using a test double approach
        // Since Directory.GetFiles is static, we test the logging behavior in isolation

        // Act
        await _converter.ConvertAsync(directory, localPaths);

        // Assert - Verify the specific LogInformation call on line 29
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async void ConvertAsync_ShouldLogConvertingEachProject()
    {
        // Arrange
        var directory = "/test/directory";
        var localPaths = new List<string> { "/test/local" };

        // Act
        await _converter.ConvertAsync(directory, localPaths);

        // Assert - Verify logging for each target project
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Converting to local reference:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async void ConvertAsync_ShouldLogFinalConversionSummary()
    {
        // Arrange
        var directory = "/test/directory";
        var localPaths = new List<string> { "/test/local" };

        // Act
        await _converter.ConvertAsync(directory, localPaths);

        // Assert - Verify final summary log
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Converted") && v.ToString()!.Contains("projects to local references.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async void ConvertAsync_WithValidInputs_ShouldNotThrow()
    {
        // Arrange
        var directory = Directory.GetCurrentDirectory(); // Use real directory
        var localPaths = new List<string> { Directory.GetCurrentDirectory() };

        // Act & Assert
        await _converter.ConvertAsync(directory, localPaths);
    }
}
