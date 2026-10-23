using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsWriteTranslationJsonMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            // Setup a temporary directory with minimal files to allow the method to run without exceptions
            var tempDir = Path.Combine(Path.GetTempPath(), "TranslateCommandTest");
            Directory.CreateDirectory(tempDir);

            // Create reference culture file with minimal content
            var referenceFile = Path.Combine(tempDir, "en.json");
            File.WriteAllText(referenceFile, "{\"texts\":[{\"name\":\"key1\",\"value\":\"value1\"}]}");

            // No target file to force creation path

            // Use reflection to call private method
            var method = typeof(TranslateCommand).GetMethod("TranslateAbpTranslateInfoAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            string targetCulture = "fr";
            string referenceCulture = "en";
            bool allValues = false;
            string authKey = "fake-auth-key";

            // Act
            var task = (Task)method.Invoke(command, new object[] { tempDir, targetCulture, referenceCulture, allValues, authKey });
            await task;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Write translation json to")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            // Cleanup
            if (File.Exists(referenceFile)) File.Delete(referenceFile);
            var targetFile = Path.Combine(tempDir, "fr.json");
            if (File.Exists(targetFile)) File.Delete(targetFile);
            Directory.Delete(tempDir);
        }
    }
}
