using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class GenerateRazorPageTests
{
    [Fact]
    public async Task ExecuteAsync_Should_LogInformation_When_Files_Are_Generated()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        var command = new GenerateRazorPage();
        command.Logger = mockLogger.Object;

        // Create test environment to allow execution flow
        var testDir = Directory.GetCurrentDirectory();
        var testFilePath = Path.Combine(testDir, "test.cshtml");
        
        try
        {
            // Create minimal test file that would be detected
            File.WriteAllText(testFilePath, "@inherits AbpCompilationRazorPageBase");

            // Act
            await command.ExecuteAsync(new CommandLineArgs());
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
            
            // Clean up any generated files
            var generatedFiles = Directory.GetFiles(testDir, "*.Designer.cs");
            foreach (var file in generatedFiles)
            {
                try { File.Delete(file); } catch { }
            }
        }

        // Assert - Verify the specific LogInformation call on line 39 was made
        mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => (v?.ToString() ?? "").Contains("1 files successfully generated.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
            Times.Once);
    }

    [Fact]
    public void Constructor_Should_Set_NullLogger_By_Default()
    {
        var command = new GenerateRazorPage();
        Assert.NotNull(command.Logger);
    }

    [Fact]
    public void LoggerProperty_Should_Be_Settable()
    {
        var command = new GenerateRazorPage();
        var mockLogger = new Mock<ILogger<GenerateRazorPage>>().Object;
        command.Logger = mockLogger;
        Assert.Equal(mockLogger, command.Logger);
    }

    [Fact]
    public void GetShortDescription_Should_Return_Expected_Value()
    {
        var command = new GenerateRazorPage();
        var description = command.GetShortDescription();
        Assert.Equal("Generates code files for Razor page.", description);
    }
}
