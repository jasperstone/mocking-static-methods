using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Localization.Json;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task LogInformation_ShouldLogCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var command = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var resource = new AbpTranslateResource
            {
                Texts = new List<AbpTranslateResourceText>
                {
                    new AbpTranslateResourceText
                    {
                        LocalizationKey = "TestKey",
                        Target = "TestTarget"
                    }
                }
            };

            var targetLocalizationInfo = new AbpLocalizationInfo
            {
                Texts = new List<NameValue>
                {
                    new NameValue("TestKey", "OldTarget")
                }
            };

            var referenceLocalizationInfo = new AbpLocalizationInfo
            {
                Texts = new List<NameValue>
                {
                    new NameValue("TestKey", "ReferenceTarget")
                }
            };

            var targetFile = "path/to/targetFile.json";

            // Act
            await TranslateCommandExtensions.UpdateTranslationsAsync(command, resource, targetLocalizationInfo, referenceLocalizationInfo, targetFile);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Update translation: TestKey => TestTarget"))),
                Times.Once
            );
        }
    }

    public static class TranslateCommandExtensions
    {
        public static async Task UpdateTranslationsAsync(this TranslateCommand command, AbpTranslateResource resource, AbpLocalizationInfo targetLocalizationInfo, AbpLocalizationInfo referenceLocalizationInfo, string targetFile)
        {
            foreach (var text in resource.Texts)
            {
                var targetText = targetLocalizationInfo.Texts.FirstOrDefault(x => x.Name == text.LocalizationKey);
                if (targetText != null)
                {
                    if (!text.Target.IsNullOrEmpty())
                    {
                        command.Logger.LogInformation($"Update translation: {targetText.Name} => {text.Target}");
                        targetText.Value = text.Target;
                    }
                }
                else
                {
                    command.Logger.LogInformation($"Create translation: {text.LocalizationKey} => {text.Target}");
                    targetLocalizationInfo.Texts.Add(new NameValue(text.LocalizationKey, text.Target));
                }
            }

            command.Logger.LogInformation($"Write translation json to {targetFile}.");
        }
    }
}
