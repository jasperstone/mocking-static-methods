using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_With_Correct_Count()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = mockLogger.Object
            };

            // Mock Directory.GetCurrentDirectory
            var currentDir = Directory.GetCurrentDirectory();
            var mockResults = new List<RazorPageGeneratorResult>
            {
                new RazorPageGeneratorResult { FilePath = "path1.cshtml", GeneratedCode = "code1" },
                new RazorPageGeneratorResult { FilePath = "path2.cshtml", GeneratedCode = "code2" }
            };

            // Mock MainCore to return predefined results
            var mainCoreCalled = false;
            generateRazorPage.GetType().GetMethod("MainCore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .CreateDelegate<Func<RazorProjectEngine, string, List<RazorPageGeneratorResult>>>(
                    (engine, dir) =>
                    {
                        mainCoreCalled = true;
                        return mockResults;
                    });

            // Act
            await generateRazorPage.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(
                x => x.LogInformation($"{mockResults.Count} files successfully generated."),
                Times.Once);
        }
    }
}
