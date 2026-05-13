using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;

namespace Volo.Abp.Cli.Tests
{
    public class LocalReferenceConverterTests
    {
        [Fact]
        public async Task ConvertAsync_ShouldLogInformationCalls()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter
            {
                Logger = mockLogger.Object
            };

            var testDirectory = Path.GetTempPath();
            var localPaths = new List<string> { Path.GetTempPath() };
            var testProjectPath = Path.Combine(testDirectory, "TestProject.csproj");
            File.WriteAllText(testProjectPath, "<Project></Project>");

            // Act
            await converter.ConvertAsync(testDirectory, localPaths);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);

            // Cleanup
            if (File.Exists(testProjectPath))
            {
                File.Delete(testProjectPath);
            }
        }
    }
}
