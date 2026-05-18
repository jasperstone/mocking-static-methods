using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsWriteTranslationJson()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            // Setup a temporary directory with minimal files to avoid exceptions
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var referenceCulture = "en";
                var targetCulture = "fr";
                var allValues = false;
                var authKey = "dummy-auth-key";

                // Create dummy reference culture file with minimal content
                var referenceFilePath = Path.Combine(tempDir, $"{referenceCulture}.json");
                File.WriteAllText(referenceFilePath, "{\"Texts\":[{\"Name\":\"Key1\",\"Value\":\"Value1\"}]}");

                // Create dummy target culture file with minimal content
                var targetFilePath = Path.Combine(tempDir, $"{targetCulture}.json");
                File.WriteAllText(targetFilePath, "{\"Texts\":[{\"Name\":\"Key1\",\"Value\":\"\"}]}");

                // Use reflection to get the private method TranslateAbpTranslateInfoAsync
                var method = typeof(TranslateCommand).GetMethod("TranslateAbpTranslateInfoAsync", BindingFlags.NonPublic | BindingFlags.Instance);

                // Act
                await (Task)method.Invoke(command, new object[] { tempDir, targetCulture, referenceCulture, allValues, authKey });

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Write translation json to")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                // Cleanup
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }
    }
}
