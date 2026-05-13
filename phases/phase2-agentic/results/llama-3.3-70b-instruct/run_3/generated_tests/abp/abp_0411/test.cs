using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Core;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var generator = new CSharpServiceProxyGenerator(
                new CliHttpClientFactory(),
                new JsonSerializationHelper()
            );
            generator.Logger = loggerMock.Object;

            var args = new GenerateProxyArgs
            {
                WorkDirectory = Path.GetTempPath(),
                Folder = "TestFolder",
                CommandName = "TestCommand"
            };

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.AtLeastOnce
            );
        }
    }
}
