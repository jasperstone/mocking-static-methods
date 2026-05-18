using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization.Json;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Commands
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
                Command = "translate",
                Options = new AbpCommandLineOptions
                {
                    { "online", "" },
                    { "culture", "fr" },
                    { "referenceCulture", "en" },
                    { "deeplAuthKey", "authKey" }
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
