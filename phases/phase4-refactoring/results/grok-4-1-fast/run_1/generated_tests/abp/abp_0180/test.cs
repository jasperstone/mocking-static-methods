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

namespace Volo.Abp.Cli.Commands.Tests;

public class GenerateRazorPageTests
{
    [Fact]
    public void GetUsageInfo_Should_Return_Expected_Usage()
    {
        // Arrange
        var command = new GenerateRazorPage();

        // Act
        var result = command.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", result);
        Assert.Contains("abp generate-razor-page", result);
        Assert.Contains("https://abp.io/docs/latest/cli", result);
    }

    [Fact]
    public void GetShortDescription_Should_Return_Expected_Description()
    {
        // Arrange
        var command = new GenerateRazorPage();

        // Act
        var result = command.GetShortDescription();

        // Assert
        Assert.Equal("Generates code files for Razor page.", result);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogInformation_After_Generating_Files()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var command = new GenerateRazorPage()
        {
            Logger = mockLogger.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert - Verify that LogInformation was called (verifies the extension method usage)
        mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
