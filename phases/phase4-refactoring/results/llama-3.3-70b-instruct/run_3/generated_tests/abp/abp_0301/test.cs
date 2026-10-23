using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_TranslateOnline_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var args = new Volo.Abp.Cli.Args.CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
