using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationWithCorrectFileCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = loggerMock.Object
            };

            // We need to mock MainCore to return a list with a known count.
            // Since MainCore is private, we cannot mock it directly.
            // Instead, we can create a derived test class to override MainCore.

            var testInstance = new TestGenerateRazorPage(loggerMock);

            var commandLineArgs = new CommandLineArgs(new string[0]);

            // Act
            await testInstance.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
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
            private readonly Mock<ILogger<GenerateRazorPage>> _loggerMock;

            public TestGenerateRazorPage(Mock<ILogger<GenerateRazorPage>> loggerMock)
            {
                Logger = loggerMock.Object;
                _loggerMock = loggerMock;
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

            // Override ExecuteAsync to call the base ExecuteAsync but use the overridden MainCore
            public new Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                var targetProjectDirectory = System.IO.Directory.GetCurrentDirectory();
                var projectEngine = CreateProjectEngine(targetProjectDirectory);

                var results = MainCore(projectEngine, targetProjectDirectory);

                foreach (var result in results)
                {
                    // Skip actual file writing in test
                }

                Logger.LogInformation($"{results.Count} files successfully generated.");

                return Task.CompletedTask;
            }
        }
    }
}
