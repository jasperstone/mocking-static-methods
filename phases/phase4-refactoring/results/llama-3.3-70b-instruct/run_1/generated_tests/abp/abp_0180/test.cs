using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage();
            generateRazorPage.Logger = loggerMock.Object;

            // Act
            await generateRazorPage.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)((v, e) => $"1 files successfully generated.")), Times.Once);
        }
    }
}
