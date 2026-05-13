using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeepL;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class TranslateCommandTests
    {
        [Fact]
        public async Task LogInformation_ShouldBeCalled_WithCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TranslateCommand>>();
            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "culture", "fr" },
                    { "referenceCulture", "en" },
                    { "allValues", "true" },
                    { "online", "true" },
                    { "DeepLAuthKey", "test-auth-key" }
                }
            };

            var translateCommand = new TranslateCommand
            {
                Logger = mockLogger.Object
            };

            // Act
            await translateCommand.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(It.Is<string>(s => s.Contains("Write translation json to"))),
                Times.Once
            );
        }
    }
}
