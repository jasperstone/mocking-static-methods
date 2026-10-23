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
            public List<RazorPageGeneratorResult> FakeResults { get; set; } = new List<RazorPageGeneratorResult>();

            protected override List<RazorPageGeneratorResult> MainCore(RazorProjectEngine projectEngine, string targetProjectDirectory)
            {
                return FakeResults;
            }
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_WithResultsCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generator = new TestGenerateRazorPage
            {
                Logger = loggerMock.Object,
                FakeResults = new List<RazorPageGeneratorResult>
                {
                    new RazorPageGeneratorResult { FilePath = "path1", GeneratedCode = "code1" },
                    new RazorPageGeneratorResult { FilePath = "path2", GeneratedCode = "code2" }
                }
            };

            // Mock Directory.GetCurrentDirectory to a dummy path
            var currentDir = Directory.GetCurrentDirectory();
            var dirMock = new Mock<IDirectoryWrapper>();
            Directory.SetCurrentDirectory(currentDir);

            // Act
            await generator.ExecuteAsync(new CommandLineArgs());

            // Assert
            // Verify that LogInformation was called with the message containing "2 files successfully generated."
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("2 files successfully generated.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
