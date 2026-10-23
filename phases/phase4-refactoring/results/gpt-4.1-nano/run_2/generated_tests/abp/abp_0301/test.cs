using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace Volo.Abp.Cli.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_Called()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = mockLogger.Object
            };

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "targetCulture", "fr" }
                }
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Target culture"))), Times.AtLeastOnce);
        }
    }
}
