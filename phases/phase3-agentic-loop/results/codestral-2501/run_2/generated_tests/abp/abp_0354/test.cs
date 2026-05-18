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
        var mockLogger = new Mock<ILogger<LocalReferenceConverter>>();
        var converter = new LocalReferenceConverter
        {
            Logger = mockLogger.Object
        };

        var directory = "testDirectory";
        var localPaths = new List<string> { "localPath1", "localPath2" };

        var mockDirectory = new Mock<Directory>();
        mockDirectory.Setup(d => d.GetFiles(directory, "*.csproj", SearchOption.AllDirectories))
                     .Returns(new string[] { "project1.csproj", "project2.csproj" });

        // Act
        await converter.ConvertAsync(directory, localPaths);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Converting projects to local reference."))),
            Times.Once);

        mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Converted 2 projects to local references."))),
            Times.Once);
    }
}
