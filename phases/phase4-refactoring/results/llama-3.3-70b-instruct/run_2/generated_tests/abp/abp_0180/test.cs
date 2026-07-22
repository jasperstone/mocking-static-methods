using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.AtLeastOnce);
        }
    }
}
