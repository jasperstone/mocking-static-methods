using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task TranslateAbpTranslateInfoAsync_LogsWriteTranslationJson()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranslateCommand>>();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var referenceCulture = "en";
                var targetCulture = "fr";
                var allValues = false;
                var authKey = "dummy-auth-key";

                var referenceFilePath = Path.Combine(tempDir, $"{referenceCulture}.json");
                var targetFilePath = Path.Combine(tempDir, $"{targetCulture}.json");

                // Write minimal valid JSON content for localization files
                File.WriteAllText(referenceFilePath, @"{ ""texts"": [ { ""name"": ""key1"", ""value"": ""value1"" } ] }");
                File.WriteAllText(targetFilePath, @"{ ""texts"": [ { ""name"": ""key1"", ""value"": ""oldvalue"" } ] }");

                var command = new TestTranslateCommand(loggerMock.Object, referenceFilePath, targetFilePath, targetCulture, referenceCulture);

                // Act
                var method = typeof(TranslateCommand).GetMethod("TranslateAbpTranslateInfoAsync", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.NotNull(method);

                var task = (Task)method.Invoke(command, new object[] { tempDir, targetCulture, referenceCulture, allValues, authKey });
                await task;

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Write translation json to")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private class TestTranslateCommand : TranslateCommand
        {
            private readonly ILogger<TranslateCommand> _logger;
            private readonly string _referenceFilePath;
            private readonly string _targetFilePath;
            private readonly string _targetCulture;
            private readonly string _referenceCulture;

            public TestTranslateCommand(ILogger<TranslateCommand> logger, string referenceFilePath, string targetFilePath, string targetCulture, string referenceCulture)
            {
                Logger = logger;
                _logger = logger;
                _referenceFilePath = referenceFilePath;
                _targetFilePath = targetFilePath;
                _targetCulture = targetCulture;
                _referenceCulture = referenceCulture;
            }

            // Provide public wrappers to call private methods for testing
            public new IEnumerable<string> GetCultureJsonFiles(string directory, string cultureName)
            {
                if (cultureName == _referenceCulture)
                {
                    return new[] { _referenceFilePath };
                }
                return Array.Empty<string>();
            }

            public new AbpLocalizationInfo GetAbpLocalizationInfoOrNull(string filePath)
            {
                if (filePath == _referenceFilePath)
                {
                    return new AbpLocalizationInfo
                    {
                        Culture = _referenceCulture,
                        Texts = new List<NameValue>
                        {
                            new NameValue("key1", "value1")
                        }
                    };
                }
                if (filePath == _targetFilePath)
                {
                    return new AbpLocalizationInfo
                    {
                        Culture = _targetCulture,
                        Texts = new List<NameValue>
                        {
                            new NameValue("key1", "oldvalue")
                        }
                    };
                }
                return null;
            }
        }

        // Minimal stubs for AbpLocalizationInfo and NameValue to allow compilation
        private class AbpLocalizationInfo
        {
            public string Culture { get; set; }
            public List<NameValue> Texts { get; set; }
        }

        private class NameValue
        {
            public string Name { get; }
            public string Value { get; set; }

            public NameValue(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }
    }
}
