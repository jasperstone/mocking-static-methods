using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization.Json;
using Xunit;

namespace Volo.Abp.Cli.Commands
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var directory = Path.GetTempPath();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "authKey";

            var translateInfo = new AbpTranslateInfo
            {
                ReferenceCulture = referenceCulture,
                TargetCulture = targetCulture,
                Resources = new List<AbpTranslateResource>
                {
                    new AbpTranslateResource
                    {
                        ResourcePath = directory,
                        Texts = new List<AbpTranslateResourceText>
                        {
                            new AbpTranslateResourceText
                            {
                                LocalizationKey = "key",
                                Reference = "reference",
                                Target = "target"
                            }
                        }
                    }
                }
            };

            // Act
            await translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsInformationForUpdateTranslation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var directory = Path.GetTempPath();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "authKey";

            var translateInfo = new AbpTranslateInfo
            {
                ReferenceCulture = referenceCulture,
                TargetCulture = targetCulture,
                Resources = new List<AbpTranslateResource>
                {
                    new AbpTranslateResource
                    {
                        ResourcePath = directory,
                        Texts = new List<AbpTranslateResourceText>
                        {
                            new AbpTranslateResourceText
                            {
                                LocalizationKey = "key",
                                Reference = "reference",
                                Target = "target"
                            }
                        }
                    }
                }
            };

            var targetFile = Path.Combine(directory, $"{targetCulture}.json");
            File.WriteAllText(targetFile, "{\"Culture\":\"fr\",\"Texts\":[{\"Name\":\"key\",\"Value\":\"existingTarget\"}]}");

            // Act
            await translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Update translation: key => target"), Times.Once);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsInformationForCreateTranslation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var directory = Path.GetTempPath();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "authKey";

            var translateInfo = new AbpTranslateInfo
            {
                ReferenceCulture = referenceCulture,
                TargetCulture = targetCulture,
                Resources = new List<AbpTranslateResource>
                {
                    new AbpTranslateResource
                    {
                        ResourcePath = directory,
                        Texts = new List<AbpTranslateResourceText>
                        {
                            new AbpTranslateResourceText
                            {
                                LocalizationKey = "key",
                                Reference = "reference",
                                Target = "target"
                            }
                        }
                    }
                }
            };

            var targetFile = Path.Combine(directory, $"{targetCulture}.json");
            File.WriteAllText(targetFile, "{\"Culture\":\"fr\",\"Texts\":[]}");

            // Act
            await translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Create translation: key => target"), Times.Once);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsInformationForWriteTranslationJson()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand
            {
                Logger = loggerMock.Object
            };

            var directory = Path.GetTempPath();
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "authKey";

            var translateInfo = new AbpTranslateInfo
            {
                ReferenceCulture = referenceCulture,
                TargetCulture = targetCulture,
                Resources = new List<AbpTranslateResource>
                {
                    new AbpTranslateResource
                    {
                        ResourcePath = directory,
                        Texts = new List<AbpTranslateResourceText>
                        {
                            new AbpTranslateResourceText
                            {
                                LocalizationKey = "key",
                                Reference = "reference",
                                Target = "target"
                            }
                        }
                    }
                }
            };

            // Act
            await translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation($"Write translation json to {Path.Combine(directory, $"{targetCulture}.json")}.", Times.Once);
        }
    }
}
