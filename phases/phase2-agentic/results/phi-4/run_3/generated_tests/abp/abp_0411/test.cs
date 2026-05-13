using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();

            var generator = new CSharpServiceProxyGenerator(
                cliHttpClientFactoryMock.Object,
                jsonSerializerMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new GenerateProxyArgs
            {
                WorkDirectory = "test_work_directory",
                Folder = "test_folder",
                WithoutContracts = false
            };

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Create ")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
