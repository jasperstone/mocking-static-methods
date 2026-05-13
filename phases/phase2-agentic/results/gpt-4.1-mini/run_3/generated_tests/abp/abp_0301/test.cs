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
        public async Task TranslateAbpTranslateInfoAsync_LogsInformationOnUpdateCreateAndWriteTranslation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            // Setup command line args to simulate online translation with required options
            var args = new CommandLineArgs(new Dictionary<string, string>
            {
                { "--culture", "fr" },
                { "--online", "" },
                { "--deepl-auth-key", "dummykey" }
            });

            // We need to call TranslateAbpTranslateInfoAsync directly, but it's private.
            // So we use reflection to invoke it.

            var method = typeof(TranslateCommand).GetMethod("TranslateAbpTranslateInfoAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Prepare parameters for the method
            string directory = System.IO.Directory.GetCurrentDirectory();
            string targetCulture = "fr";
            string referenceCulture = "en";
            bool allValues = false;
            string authKey = "dummykey";

            // Act
            // We expect the method to log information messages including the final log line on line 228
            var task = (Task)method.Invoke(command, new object[] { directory, targetCulture, referenceCulture, allValues, authKey });
            await task;

            // Assert
            // Verify that LogInformation was called with the expected message containing "Write translation json to"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Write translation json to")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
