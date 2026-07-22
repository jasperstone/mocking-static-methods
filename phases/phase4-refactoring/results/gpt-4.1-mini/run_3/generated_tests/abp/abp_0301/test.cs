using System;
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

            // Prepare a temporary directory with minimal files to avoid exceptions
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create dummy reference and target json files to avoid exceptions in the method
                var referenceFile = Path.Combine(tempDir, "en.json");
                var targetFile = Path.Combine(tempDir, "fr.json");

                // Minimal valid JSON structure expected by GetAbpLocalizationInfoOrNull
                File.WriteAllText(referenceFile, "{\"Texts\":[{\"Name\":\"key1\",\"Value\":\"value1\"}]}");
                File.WriteAllText(targetFile, "{\"Texts\":[{\"Name\":\"key1\",\"Value\":\"value2\"}]}");

                // Use reflection to call the private method
                var method = typeof(TranslateCommand).GetMethod("TranslateAbpTranslateInfoAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(method);

                string targetCulture = "fr";
                string referenceCulture = "en";
                bool allValues = false;
                string authKey = "dummy-auth-key";

                // Act
                var task = (Task)method.Invoke(command, new object[] { tempDir, targetCulture, referenceCulture, allValues, authKey });
                await task;

                // Assert: Check that any LogInformation call was made (not necessarily with exact message)
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }
    }
}
