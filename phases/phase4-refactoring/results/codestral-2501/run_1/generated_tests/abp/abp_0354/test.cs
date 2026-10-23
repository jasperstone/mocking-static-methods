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

        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())).FullName;
        var localPaths = new List<string> { "localPath1", "localPath2" };

        // Act
        await converter.ConvertAsync(directory, localPaths);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(3));
    }
}
