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
    public async Task ConvertAsync_ShouldLogInformationMessages()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
        var localReferenceConverter = new LocalReferenceConverter
        {
            Logger = loggerMock.Object
        };

        var directory = "testDirectory";
        var localPaths = new List<string> { "localPath1", "localPath2" };

        // Act
        await localReferenceConverter.ConvertAsync(directory, localPaths);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Converting projects to local reference.")),
                It.IsAny<object[]>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Converted 0 projects to local references.")),
                It.IsAny<object[]>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()),
            Times.Once);
    }
}
