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
    [Fact]
    public async Task ConvertAsync_ShouldLogInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
        var converter = new LocalReferenceConverter
        {
            Logger = loggerMock.Object
        };

        var directory = "testDirectory";
        var localPaths = new List<string> { "localPath1", "localPath2" };

        // Act
        await converter.ConvertAsync(directory, localPaths);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converted 0 projects to local references.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
