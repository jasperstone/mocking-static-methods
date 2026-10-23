using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using System.Threading.Tasks;

public class GenerateRazorPageTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
        var generateRazorPage = new GenerateRazorPage
        {
            Logger = mockLogger.Object
        };

        var commandLineArgs = new CommandLineArgs();

        // Act
        await generateRazorPage.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("files successfully generated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
