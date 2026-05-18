using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationOnSuccess()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs();

            // Act
            await generateRazorPage.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("0 files successfully generated.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
