using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        private class TestGenerateRazorPage : GenerateRazorPage
        {
            public List<RazorPageGeneratorResult> ResultsToReturn { get; set; }
            public string FakeDirectory { get; set; }

            public TestGenerateRazorPage(List<RazorPageGeneratorResult> results, string directory)
            {
                ResultsToReturn = results;
                FakeDirectory = directory;
            }

            protected override List<RazorPageGeneratorResult> MainCore(RazorProjectEngine projectEngine, string targetProjectDirectory)
            {
                return ResultsToReturn;
            }
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformation_WhenFilesGenerated()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
            var results = new List<RazorPageGeneratorResult>
            {
                new RazorPageGeneratorResult { FilePath = "file1.cshtml", GeneratedCode = "code1" },
                new RazorPageGeneratorResult { FilePath = "file2.cshtml", GeneratedCode = "code2" }
            };
            var fakeDirectory = "/fake/directory";

            var command = new TestGenerateRazorPage(results, fakeDirectory)
            {
                Logger = mockLogger.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(
                x => x.LogInformation($"{results.Count} files successfully generated."),
                Times.Once);
        }
    }
}
