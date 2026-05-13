using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Localization.Json;
using Xunit;

namespace Volo.Abp.Cli.Tests
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
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsCreateTranslation()
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
            loggerMock.Verify(l => l.LogInformation($"Create translation: key => target"), Times.Once);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsUpdateTranslation()
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
            loggerMock.Verify(l => l.LogInformation($"Update translation: key => target"), Times.Once);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsWriteTranslationJson()
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
            loggerMock.Verify(l => l.LogInformation($"Write translation json to {directory}/{targetCulture}.json."), Times.Once);
        }
    }
}
