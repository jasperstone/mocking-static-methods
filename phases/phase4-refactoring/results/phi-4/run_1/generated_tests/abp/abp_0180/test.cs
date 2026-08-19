using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationOnSuccess()
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
                logger => logger.LogInformation(It.Is<string>(s => s.Contains("files successfully generated."))),
                Times.Once);
        }
    }
}
