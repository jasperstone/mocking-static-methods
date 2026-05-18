using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

public class GenerateRazorPageTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogCorrectInformation()
    {
        // Arrange
        var logMessages = new List<string>();
        var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<object>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<object, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                (logLevel, eventId, state, exception, formatter) =>
                {
                    logMessages.Add(state.ToString());
                });

        var commandLineArgs = new CommandLineArgs();

        var generateRazorPage = new GenerateRazorPage
        {
            Logger = loggerMock.Object
        };

        // Act
        await generateRazorPage.ExecuteAsync(commandLineArgs);

        // Assert
        Assert.Contains(logMessages, x => x.Contains("files successfully generated."));
    }

    [Fact]
    public void GetUsageInfo_ShouldReturnCorrectUsageInfo()
    {
        // Arrange
        var generateRazorPage = new GenerateRazorPage();

        // Act
        var usageInfo = generateRazorPage.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", usageInfo);
        Assert.Contains("abp generate-razor-page", usageInfo);
        Assert.Contains("See the documentation for more info: https://abp.io/docs/latest/cli", usageInfo);
    }

    [Fact]
    public void GetShortDescription_ShouldReturnCorrectDescription()
    {
        // Arrange
        var generateRazorPage = new GenerateRazorPage();

        // Act
        var shortDescription = generateRazorPage.GetShortDescription();

        // Assert
        Assert.Equal("Generates code files for Razor page.", shortDescription);
    }
}
