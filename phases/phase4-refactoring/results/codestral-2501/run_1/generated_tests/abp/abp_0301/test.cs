using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Localization.Json;
using Volo.Abp.Options;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task ShouldLogInformationWhenWritingTranslationJson()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { Options.Culture.Long, "fr" },
                    { Options.DeepLAuthKey.Long, "authKey" },
                    { Options.Online.Long, "" }
                }
            };

            var currentDirectory = Directory.GetCurrentDirectory();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = false;
            var authKey = "authKey";

            var translateInfo = new AbpTranslateInfo
            {
                ReferenceCulture = referenceCulture,
                TargetCulture = targetCulture,
                Resources = new List<AbpTranslateResource>
                {
                    new AbpTranslateResource
                    {
                        ResourcePath = currentDirectory,
                        Texts = new List<AbpTranslateResourceText>
                        {
                            new AbpTranslateResourceText
                            {
                                LocalizationKey = "Key1",
                                Reference = "Reference1",
                                Target = "Target1"
                            }
                        }
                    }
                }
            };

            var targetFile = Path.Combine(currentDirectory, $"{targetCulture}.json");
            var targetLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = targetCulture,
                Texts = new List<NameValue>
                {
                    new NameValue("Key1", "Target1")
                }
            };

            // Act
            await command.TranslateAbpTranslateInfoAsync(currentDirectory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Write translation json to {targetFile}.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
