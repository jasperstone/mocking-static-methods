using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Http;
using Volo.Abp.Http.Modeling;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_LogsInformation_WhenRemoveProxyCommand()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null);
            var mockJsonSerializer = new Mock<IJsonSerializer>(MockBehavior.Strict);

            var generator = new CSharpServiceProxyGenerator(mockCliHttpClientFactory.Object, mockJsonSerializer.Object);
            // Inject the mock logger
            typeof(CSharpServiceProxyGenerator)
                .GetProperty("Logger", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                ?.SetValue(generator, mockLogger.Object);

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var args = new GenerateProxyArgs
            {
                CommandName = RemoveProxyCommand.Name,
                WorkDirectory = tempDir,
                Folder = null
            };

            // Create the folder to be deleted
            var folderPath = Path.Combine(tempDir, "ClientProxies");
            Directory.CreateDirectory(folderPath);

            // Expect LogInformation to be called with a message containing "Delete"
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Delete")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify();
            Assert.False(Directory.Exists(folderPath)); // Folder should be deleted

            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
