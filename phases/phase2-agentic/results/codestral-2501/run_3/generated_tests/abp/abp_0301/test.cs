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
        public async Task TranslateAbpTranslateInfoAsync_ShouldLogInformation()
        {
            // Arrange
            var directory = "testDirectory";
            var targetCulture = "fr";
            var referenceCulture = "en";
            var allValues = true;
            var authKey = "testAuthKey";

            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "online", "" },
                    { "culture", targetCulture },
                    { "referenceCulture", referenceCulture },
                    { "allValues", "" },
                    { "deeplAuthKey", authKey }
                }
            };

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
            var referenceFile = Path.Combine(directory, $"{referenceCulture}.json");

            var targetLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = targetCulture,
                Texts = new List<NameValue>()
            };

            var referenceLocalizationInfo = new AbpLocalizationInfo
            {
                Culture = referenceCulture,
                Texts = new List<NameValue>()
            };

            // Mock file existence and content
            var fileSystemMock = new Mock<FileSystem>();
            fileSystemMock.Setup(fs => fs.FileExists(targetFile)).Returns(true);
            fileSystemMock.Setup(fs => fs.FileExists(referenceFile)).Returns(true);
            fileSystemMock.Setup(fs => fs.ReadAllText(targetFile)).Returns(JsonConvert.SerializeObject(targetLocalizationInfo));
            fileSystemMock.Setup(fs => fs.ReadAllText(referenceFile)).Returns(JsonConvert.SerializeObject(referenceLocalizationInfo));

            // Act
            await _translateCommand.TranslateAbpTranslateInfoAsync(directory, targetCulture, referenceCulture, allValues, authKey);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(4));
        }
    }

    public class FileSystem
    {
        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
