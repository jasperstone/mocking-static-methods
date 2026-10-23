using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_With_Correct_Message()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var command = new GenerateRazorPage
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var originalDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(tempDir);

            var razorFilePath = Path.Combine(tempDir, "TestPage.cshtml");
            File.WriteAllText(razorFilePath, "@inherits AbpCompilationRazorPageBase");

            try
            {
                // Act
                await command.ExecuteAsync(args);

                // Assert
                string loggedMessage = null;
                loggerMock.Verify(
                    x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => {
                            loggedMessage = v.ToString();
                            return loggedMessage.Contains(" files successfully generated.");
                        }),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce);

                Assert.NotNull(loggedMessage);
                Assert.Contains(" files successfully generated.", loggedMessage);
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
                Directory.Delete(tempDir, true);
            }
        }
    }
}
