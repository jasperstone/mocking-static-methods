using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenFilesGenerated()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var command = new GenerateRazorPage
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void GetUsageInfo_ShouldReturnCorrectUsageInfo()
        {
            // Arrange
            var command = new GenerateRazorPage();

            // Act
            var usageInfo = command.GetUsageInfo();

            // Assert
            Assert.Contains("Usage:", usageInfo);
            Assert.Contains("abp generate-razor-page", usageInfo);
            Assert.Contains("https://abp.io/docs/latest/cli", usageInfo);
        }

        [Fact]
        public void GetShortDescription_ShouldReturnCorrectDescription()
        {
            // Arrange
            var command = new GenerateRazorPage();

            // Act
            var description = command.GetShortDescription();

            // Assert
            Assert.Equal("Generates code files for Razor page.", description);
        }
    }
}
