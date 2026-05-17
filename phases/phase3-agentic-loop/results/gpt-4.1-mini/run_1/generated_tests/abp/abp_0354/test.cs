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
        public async Task ConvertAsync_LogsInformationAtExpectedPoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter
            {
                Logger = loggerMock.Object
            };

            // We will use a temp directory with no csproj files to keep it simple
            var tempDir = System.IO.Path.GetTempPath();

            // Provide a localPaths list with a directory that exists but no csproj files
            var localPaths = new List<string> { tempDir };

            // Act
            await converter.ConvertAsync(tempDir, localPaths);

            // Assert
            // Check that the initial log message was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);

            // Check that the final log message was called (converted 0 projects)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converted 0 projects to local references.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);
        }
    }
}
