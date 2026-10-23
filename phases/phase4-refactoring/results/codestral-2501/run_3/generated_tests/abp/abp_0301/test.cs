using System;
using System.Collections.Generic;
using System.IO;
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

            var translateInfo = new AbpTranslateInfo
            {
                ReferenceCulture = referenceCulture,
                TargetCulture = targetCulture,
                Resources = new List<AbpTranslateResource>()
            };

            var resource = new AbpTranslateResource
            {
                ResourcePath = directory,
                Texts = new List<AbpTranslateResourceText>()
            };

            translateInfo.Resources.Add(resource);

            var targetFile = Path.Combine(directory, $"{targetCulture}.json");
            var targetLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = targetCulture,
                Texts = new List<NameValue>()
            };

            var referenceFile = Path.Combine(directory, $"{referenceCulture}.json");
            var referenceLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = referenceCulture,
                Texts = new List<NameValue>()
            };

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
