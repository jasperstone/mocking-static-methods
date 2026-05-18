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
using Volo.Abp.Localization.Json;
using Volo.Abp.Options;
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
        public async Task ExecuteAsync_ShouldLogInformation_WhenTranslating()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new string[]
            {
                "translate",
                "--culture", "fr",
                "--deepl-auth-key", "authKey",
                "--online"
            });

            var directory = Directory.GetCurrentDirectory();
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
                        ResourcePath = directory,
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

            var targetFile = Path.Combine(directory, $"{targetCulture}.json");
            var targetLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = targetCulture,
                Texts = new List<NameValue>
                {
                    new NameValue("Key1", "Target1")
                }
            };

            var referenceFile = Path.Combine(directory, $"{referenceCulture}.json");
            var referenceLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = referenceCulture,
                Texts = new List<NameValue>
                {
                    new NameValue("Key1", "Reference1")
                }
            };

            // Act
            await _translateCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Write translation json to")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
