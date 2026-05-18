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

        var mockFileSystem = new Mock<FileSystem>();
        mockFileSystem.Setup(fs => fs.GetFiles(directory, "*.csproj", SearchOption.AllDirectories))
                      .Returns(new string[] { "project1.csproj", "project2.csproj" });

        // Act
        await converter.ConvertAsync(directory, localPaths);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Exactly(3));
    }
}
