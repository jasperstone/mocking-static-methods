using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_With_Correct_Count()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
            var dummyResults = new List<GenerateRazorPage.RazorPageGeneratorResult>
            {
                new GenerateRazorPage.RazorPageGeneratorResult { FilePath = "file1.cshtml", GeneratedCode = "code1" },
                new GenerateRazorPage.RazorPageGeneratorResult { FilePath = "file2.cshtml", GeneratedCode = "code2" }
            };
            var testInstance = new TestGenerateRazorPage(mockLogger.Object, dummyResults);

            // Act
            await testInstance.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(
                x => x.LogInformation($"{dummyResults.Count} files successfully generated."),
                Times.Once);
        }

        private class TestGenerateRazorPage : GenerateRazorPage
        {
            private readonly List<RazorPage.RazorPageGeneratorResult> _results;

            public TestGenerateRazorPage(ILogger<GenerateRazorPage> logger, List<RazorPage.RazorPageGeneratorResult> results)
            {
                Logger = logger;
                _results = results;
            }

            protected override List<RazorPage.RazorPageGeneratorResult> MainCore(RazorProjectEngine projectEngine, string targetProjectDirectory)
            {
                return _results;
            }
        }
    }
}
