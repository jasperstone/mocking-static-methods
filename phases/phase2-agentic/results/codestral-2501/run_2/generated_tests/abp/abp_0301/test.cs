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
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
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
        public async Task ExecuteAsync_ShouldLogInformation_WhenTranslatingOnline()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "c", "fr" },
                    { "r", "en" },
                    { "online", "" },
                    { "authKey", "fakeAuthKey" }
                }
            };

            // Act
            await _translateCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Abp translate online...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Target culture: fr")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reference culture: en")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenGeneratingAbpTranslateInfo()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "c", "fr" },
                    { "r", "en" },
                    { "o", "output.json" }
                }
            };

            // Act
            await _translateCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Abp translate...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Target culture: fr")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reference culture: en")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Output file: output.json")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The translation file has been created.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_ShouldLogInformation_WhenUpdatingTranslations()
        {
            // Arrange
            var directory = "testDirectory";
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "fakeAuthKey";

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

            var targetLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = targetCulture,
                Texts = new List<NameValue>
                {
                    new NameValue("Key1", "Target1")
                }
            };

            var referenceLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = referenceCulture,
                Texts = new List<NameValue>
                {
                    new NameValue("Key1", "Reference1")
                }
            };

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(Path.Combine(directory, $"{targetCulture}.json"), new MockFileData(JsonConvert.SerializeObject(targetLocalizationInfo)));
            mockFileSystem.AddFile(Path.Combine(directory, $"{referenceCulture}.json"), new MockFileData(JsonConvert.SerializeObject(referenceLocalizationInfo)));

            var mockTranslator = new Mock<Translator>(authKey);
            mockTranslator.Setup(t => t.TranslateTextAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TranslationResult[]
                {
                    new TranslationResult { Text = "Target1" }
                });

            // Act
            await _translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update translation: Key1 => Target1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_ShouldLogInformation_WhenCreatingTranslations()
        {
            // Arrange
            var directory = "testDirectory";
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "fakeAuthKey";

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
                                LocalizationKey = "Key2",
                                Reference = "Reference2",
                                Target = "Target2"
                            }
                        }
                    }
                }
            };

            var targetLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = targetCulture,
                Texts = new List<NameValue>()
            };

            var referenceLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = referenceCulture,
                Texts = new List<NameValue>
                {
                    new NameValue("Key2", "Reference2")
                }
            };

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(Path.Combine(directory, $"{referenceCulture}.json"), new MockFileData(JsonConvert.SerializeObject(referenceLocalizationInfo)));

            var mockTranslator = new Mock<Translator>(authKey);
            mockTranslator.Setup(t => t.TranslateTextAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TranslationResult[]
                {
                    new TranslationResult { Text = "Target2" }
                });

            // Act
            await _translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create translation: Key2 => Target2")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_ShouldLogInformation_WhenWritingTranslationJson()
        {
            // Arrange
            var directory = "testDirectory";
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "fakeAuthKey";

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

            var targetLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = targetCulture,
                Texts = new List<NameValue>
                {
                    new NameValue("Key1", "Target1")
                }
            };

            var referenceLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = referenceCulture,
                Texts = new List<NameValue>
                {
                    new NameValue("Key1", "Reference1")
                }
            };

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(Path.Combine(directory, $"{targetCulture}.json"), new MockFileData(JsonConvert.SerializeObject(targetLocalizationInfo)));
            mockFileSystem.AddFile(Path.Combine(directory, $"{referenceCulture}.json"), new MockFileData(JsonConvert.SerializeObject(referenceLocalizationInfo)));

            var mockTranslator = new Mock<Translator>(authKey);
            mockTranslator.Setup(t => t.TranslateTextAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TranslationResult[]
                {
                    new TranslationResult { Text = "Target1" }
                });

            // Act
            await _translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Write translation json to {Path.Combine(directory, $"{targetCulture}.json")}.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
