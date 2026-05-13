using System.Collections.Generic;
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
        public async Task TranslateAbpTranslateInfoAsync_LogsInformationIncludingWriteTranslationJson()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            // Setup minimal data for TranslateAbpTranslateInfoAsync to run without exceptions
            // We will call the method via reflection because it's private
            var directory = System.IO.Directory.GetCurrentDirectory();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = false;
            var authKey = "dummy-auth-key";

            // We need to mock or override some methods or file system calls to avoid exceptions
            // But since we cannot modify the class, we will just test that Logger.LogInformation is called with the expected message on line 228

            // Act
            // Use reflection to invoke private method TranslateAbpTranslateInfoAsync
            var method = typeof(TranslateCommand).GetMethod("TranslateAbpTranslateInfoAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Because the method is async Task, invoke returns Task
            var task = (Task)method.Invoke(command, new object[] { directory, targetCulture, referenceCulture, allValues, authKey });
            await task;

            // Assert
            // Verify that Logger.LogInformation was called with the message containing "Write translation json to"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Write translation json to")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
