using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
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
        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_ShouldLogInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs(
                "translate",
                new List<string>(),
                new Dictionary<string, string>
                {
                    { Options.Culture.Long, "fr" },
                    { Options.DeepLAuthKey.Long, "authKey" },
                    { Options.Online.Long, "" }
                }
            );

            var directory = Directory.GetCurrentDirectory();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = false;
            var authKey = "authKey";

            // Act
            await command.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Write translation json to")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
