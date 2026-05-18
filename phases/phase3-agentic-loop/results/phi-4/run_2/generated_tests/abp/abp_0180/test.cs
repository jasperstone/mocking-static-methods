using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationOnSuccess()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = mockLogger.Object
            };

            var mockCommandLineArgs = new Mock<CommandLineArgs>();
            var mockProjectEngine = new Mock<RazorProjectEngine>();
            var mockFileSystem = new Mock<RazorProjectFileSystem>();
            mockFileSystem.Setup(fs => fs.EnumerateItems(It.IsAny<string>()))
                .Returns(new List<RazorProjectItem>()); // Simulate no files found

            mockProjectEngine.Setup(pe => pe.FileSystem).Returns(mockFileSystem.Object);

            // Act
            await generateRazorPage.ExecuteAsync(mockCommandLineArgs.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(It.Is<string>(s => s.Contains("No .cshtml or .razor files were found."))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsGeneratedFilesCount()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = mockLogger.Object
            };

            var mockCommandLineArgs = new Mock<CommandLineArgs>();
            var mockProjectEngine = new Mock<RazorProjectEngine>();
            var mockFileSystem = new Mock<RazorProjectFileSystem>();
            var mockRazorProjectItem = new Mock<RazorProjectItem>();
            mockRazorProjectItem.Setup(item => item.PhysicalPath).Returns("Test.cshtml");

            mockFileSystem.Setup(fs => fs.EnumerateItems(It.IsAny<string>()))
                .Returns(new List<RazorProjectItem> { mockRazorProjectItem.Object });

            mockProjectEngine.Setup(pe => pe.FileSystem).Returns(mockFileSystem.Object);

            // Act
            await generateRazorPage.ExecuteAsync(mockCommandLineArgs.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(It.Is<string>(s => s.Contains("1 files successfully generated."))),
                Times.Once);
        }
    }
}
