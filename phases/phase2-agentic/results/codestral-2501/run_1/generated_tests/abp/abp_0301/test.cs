using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Localization.Json;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class TranslateCommandTests
    {
        private readonly Mock<ILogger<TranslateCommand>> _loggerMock;
        private readonly TranslateCommand _translateCommand;

        public TranslateCommandTests()
        {
            _loggerMock = new Mock<ILogger<TranslateCommand>>();
            _translateCommand = new TranslateCommand
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenWritingTranslationJson()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "online", "" },
                    { "culture", "fr" },
                    { "deepl-auth-key", "authKey" }
                }
            };

            var currentDirectory = Directory.GetCurrentDirectory();
            var targetFile = Path.Combine(currentDirectory, "fr.json");

            // Act
            await _translateCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Write translation json to {targetFile}.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
