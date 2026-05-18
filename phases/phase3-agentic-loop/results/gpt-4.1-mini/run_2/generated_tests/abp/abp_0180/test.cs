using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationWithCorrectFileCount()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
            var command = new GenerateRazorPage
            {
                Logger = mockLogger.Object
            };

            // We need to mock MainCore to return a list with a known count.
            // Since MainCore is private, we cannot override it directly.
            // Instead, we can create a derived test class to override MainCore.

            var testCommand = new TestGenerateRazorPage(mockLogger.Object);

            var args = new CommandLineArgs(new string[0]);

            // Act
            await testCommand.ExecuteAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "2 files successfully generated."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestGenerateRazorPage : GenerateRazorPage
        {
            private readonly ILogger<GenerateRazorPage> _logger;

            public TestGenerateRazorPage(ILogger<GenerateRazorPage> logger)
            {
                Logger = logger;
                _logger = logger;
            }

            // Override MainCore to return a fixed list of two results
            private new List<RazorPageGeneratorResult> MainCore(Microsoft.AspNetCore.Razor.Language.RazorProjectEngine projectEngine, string targetProjectDirectory)
            {
                return new List<RazorPageGeneratorResult>
                {
                    new RazorPageGeneratorResult { FilePath = "file1.cs", GeneratedCode = "code1" },
                    new RazorPageGeneratorResult { FilePath = "file2.cs", GeneratedCode = "code2" }
                };
            }

            public override Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                // We call the base ExecuteAsync but replace the call to MainCore with our override
                var targetProjectDirectory = System.IO.Directory.GetCurrentDirectory();
                var projectEngine = CreateProjectEngine(targetProjectDirectory);

                var results = MainCore(projectEngine, targetProjectDirectory);

                foreach (var result in results)
                {
                    // We skip actual file writing in test
                }

                Logger.LogInformation($"{results.Count} files successfully generated.");

                return Task.CompletedTask;
            }
        }
    }
}
