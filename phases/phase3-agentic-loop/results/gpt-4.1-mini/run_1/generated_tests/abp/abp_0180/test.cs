using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var command = new GenerateRazorPage
            {
                Logger = loggerMock.Object
            };

            var args = CommandLineArgs.Empty();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("files successfully generated.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
