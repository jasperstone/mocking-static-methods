using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Core;
using Volo.Abp.Cli.Core.Commands;
using Volo.Abp.Localization;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task TranslateCommand_LogInformation_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand(loggerMock.Object);

            // Act
            await translateCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TranslateCommand_TranslateAbpTranslateInfoAsync_LogInformation_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand(loggerMock.Object);
            var directory = Directory.GetCurrentDirectory();
            var targetCulture = "en";
            var referenceCulture = "en";
            var allValues = false;
            var authKey = "authKey";

            // Act
            await translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TranslateCommand_TranslateAbpTranslateInfoAsync_CreateTranslation_LogInformation_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand(loggerMock.Object);
            var directory = Directory.GetCurrentDirectory();
            var targetCulture = "en";
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
        public async Task TranslateCommand_TranslateAbpTranslateInfoAsync_UpdateTranslation_LogInformation_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var translateCommand = new TranslateCommand(loggerMock.Object);
            var directory = Directory.GetCurrentDirectory();
            var targetCulture = "en";
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
    }
}
