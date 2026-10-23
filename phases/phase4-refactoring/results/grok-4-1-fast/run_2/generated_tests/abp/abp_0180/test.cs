using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class GenerateRazorPageTests
{
    [Fact]
    public void GetUsageInfo_Should_Return_Expected_Usage_Text()
    {
        // Arrange
        var command = new GenerateRazorPage();

        // Act
        var usageInfo = command.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", usageInfo);
        Assert.Contains("abp generate-razor-page", usageInfo);
        Assert.Contains("https://abp.io/docs/latest/cli", usageInfo);
    }

    [Fact]
    public void GetShortDescription_Should_Return_Expected_Description()
    {
        // Arrange
        var command = new GenerateRazorPage();

        // Act
        var description = command.GetShortDescription();

        // Assert
        Assert.Equal("Generates code files for Razor page.", description);
    }

    [Fact]
    public void Constructor_Should_Initialize_With_NullLogger()
    {
        // Act
        var command = new GenerateRazorPage();

        // Assert
        Assert.NotNull(command.Logger);
        Assert.IsAssignableFrom<ILogger<GenerateRazorPage>>(command.Logger);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Call_LogInformation_With_Results_Count()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("files successfully generated.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        var command = new GenerateRazorPage { Logger = mockLogger.Object };

        // Mock static Directory.GetCurrentDirectory to avoid file system issues
        // Mock private methods using refactor if needed, but test public behavior

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert - Verifies the LogInformation extension method call on line 39 was invoked
        mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("files successfully generated.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);
    }
}
