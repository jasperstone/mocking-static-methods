using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using System.IO;
using System.Linq;

namespace Volo.Abp.Cli.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task LogInformation_Called_On_Line_228()
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
                    { "culture", "fr" }
                }
            };

            // Setup a minimal context to reach line 228
            // We need to simulate the online translation branch
            // For that, we set Options.Online.Long key
            args.Options[Options.Online.Long] = "true";
            args.Options[Options.DeepLAuthKey.Short] = "dummy";

            // Act
            await command.ExecuteAsync(args);

            // Assert
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Abp translate online..."))), Times.Once);
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Target culture:"))), Times.Once);
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Reference culture:"))), Times.Once);
        }
    }
}
