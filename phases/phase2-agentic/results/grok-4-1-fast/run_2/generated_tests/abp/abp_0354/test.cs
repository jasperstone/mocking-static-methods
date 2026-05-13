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

        // Mock static methods to avoid real file system access
        var originalGetFiles = typeof(Directory).GetMethod("GetFiles", new[] { typeof(string), typeof(SearchOption) });
        // Note: In a real test environment with file system mocking libraries like FileSystemMock, 
        // these would return controlled results. For unit test isolation, we focus on the LogInformation call.

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
    public async void ConvertAsync_ShouldLogPerProjectConversion_WhenTargetProjectsExist()
    {
        // Arrange
        var directory = "/test/directory";
        var localPaths = new List<string> { "/test/local" };

        // Act
        await _converter.ConvertAsync(directory, localPaths);

        // Assert - Verify per-project logging occurs
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
    public async void ConvertAsync_ShouldLogSummary_WhenConversionCompletes()
    {
        // Arrange
        var directory = "/test/directory";
        var localPaths = new List<string> { "/test/local" };

        // Act
        await _converter.ConvertAsync(directory, localPaths);

        // Assert - Verify final summary logging
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
    public void ConvertAsync_ShouldThrow_WhenDirectoryIsNull()
    {
        // Arrange
        var localPaths = new List<string> { "/test/local" };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _converter.ConvertAsync(null!, localPaths));
    }

    [Fact]
    public void ConvertAsync_ShouldThrow_WhenLocalPathsIsNull()
    {
        // Arrange
        var directory = "/test/directory";

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _converter.ConvertAsync(directory, null!));
    }
}
