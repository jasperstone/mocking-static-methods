using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests
{
    public class LocalReferenceConverterTests
    {
        [Fact]
        public async Task ConvertAsync_LogsInformationAtStartAndEndAndForEachProject()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter
            {
                Logger = loggerMock.Object
            };

            // We will create a temporary directory and files for testing
            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LocalReferenceConverterTest");
            if (System.IO.Directory.Exists(tempDir))
            {
                System.IO.Directory.Delete(tempDir, true);
            }
            System.IO.Directory.CreateDirectory(tempDir);

            // Create dummy .csproj files in the directory
            var project1 = System.IO.Path.Combine(tempDir, "Project1.csproj");
            var project2 = System.IO.Path.Combine(tempDir, "Project2.csproj");
            System.IO.File.WriteAllText(project1, "<Project></Project>");
            System.IO.File.WriteAllText(project2, "<Project></Project>");

            var localPaths = new List<string> { tempDir };

            // Act
            await converter.ConvertAsync(tempDir, localPaths);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting to local reference:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Exactly(2));

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converted 2 projects to local references.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);

            // Cleanup
            System.IO.Directory.Delete(tempDir, true);
        }
    }
}
