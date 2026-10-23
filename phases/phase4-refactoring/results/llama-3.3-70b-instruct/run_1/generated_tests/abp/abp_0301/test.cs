using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
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
            var args = new CommandLineArgs("translate", null);
            args.Options.Add("--online", "");
            args.Options.Add("--culture", "fr");
            args.Options.Add("--reference-culture", "en");
            args.Options.Add("--deep-l-auth-key", "auth-key");

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
