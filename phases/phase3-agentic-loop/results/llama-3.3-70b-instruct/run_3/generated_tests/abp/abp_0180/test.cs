using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public void ExecuteAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage();
            generateRazorPage.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs();

            // Act
            generateRazorPage.ExecuteAsync(commandLineArgs).Wait();

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ExecuteAsync_GeneratesFiles()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage();
            generateRazorPage.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs();
            var targetProjectDirectory = Directory.GetCurrentDirectory();

            // Act
            generateRazorPage.ExecuteAsync(commandLineArgs).Wait();

            // Assert
            var files = Directory.EnumerateFiles(targetProjectDirectory, "*.cs");
            Assert.True(files.Any());
        }
    }
}
