using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

public class LocalReferenceConverterTests
{
    private readonly LocalReferenceConverter _converter;
    private readonly Mock<ILogger<LocalReferenceConverter>> _loggerMock;

    public LocalReferenceConverterTests()
    {
        _converter = new LocalReferenceConverter();
        _loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
        _converter.Logger = _loggerMock.Object;
    }

    [Fact]
    public async Task ConvertAsync_ShouldLogInformation_WhenConvertingProjects()
    {
        // Arrange
        var directory = "testDirectory";
        var localPaths = new List<string> { "localPath1", "localPath2" };
        var targetProjects = new[] { "project1.csproj", "project2.csproj" };

        Directory.SetCurrentDirectory("testDirectory");
        Directory.CreateDirectory("testDirectory");
        File.WriteAllText("project1.csproj", "<Project></Project>");
        File.WriteAllText("project2.csproj", "<Project></Project>");

        // Act
        await _converter.ConvertAsync(directory, localPaths);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting to local reference: project1.csproj")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting to local reference: project2.csproj")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converted 2 projects to local references.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
