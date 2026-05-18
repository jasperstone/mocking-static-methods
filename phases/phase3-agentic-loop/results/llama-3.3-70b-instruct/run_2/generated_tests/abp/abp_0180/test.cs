using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = loggerMock.Object
            };

            // Act
            await generateRazorPage.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = loggerMock.Object
            };

            // Act
            await generateRazorPage.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("0 files successfully generated.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }
    }
}
