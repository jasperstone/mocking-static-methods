using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class GenerateRazorPageTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WithResultsCount()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
        mockLogger.SetupAllProperties();

        var testDir = Path.Combine(Path.GetTempPath(), "razor-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(testDir);
        var originalDir = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(testDir);

            // Create dummy cshtml file that MainCore will find
            var dummyCshtml = Path.Combine(testDir, "TestPage.cshtml");
            await File.WriteAllTextAsync(dummyCshtml, "@inherits AbpCompilationRazorPageBase");

            var command = new GenerateRazorPage();
            command.Logger = mockLogger.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert - verify the specific LogInformation call
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("1 files successfully generated.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}
