using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectModification;

namespace Volo.Abp.Cli.Tests
{
    public class LocalReferenceConverterTests
    {
        [Fact]
        public async Task ConvertAsync_Should_LogInformationCalls()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter
            {
                Logger = loggerMock.Object
            };

            // Create a temporary directory with a dummy csproj file
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csprojPath = Path.Combine(tempDir, "TestProject.csproj");
            File.WriteAllText(csprojPath, "<Project></Project>");

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
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
