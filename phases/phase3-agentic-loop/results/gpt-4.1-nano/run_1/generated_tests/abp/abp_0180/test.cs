using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_With_Correct_Count()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = loggerMock.Object
            };

            // Setup current directory
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            Directory.SetCurrentDirectory(tempDir);

            // Create dummy Razor file
            var razorFilePath = Path.Combine(tempDir, "TestPage.cshtml");
            File.WriteAllText(razorFilePath, "@inherits AbpCompilationRazorPageBase");

            // Act
            await generateRazorPage.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(msg => msg.Contains("files successfully generated"))),
                Times.Once);

            // Cleanup
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            Directory.Delete(tempDir, true);
        }
    }
}
