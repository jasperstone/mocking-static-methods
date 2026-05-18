using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.Commands;
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
            // Inject the mock logger into the base class Logger property via reflection
            var loggerField = typeof(ServiceProxyGeneratorBase<CSharpServiceProxyGenerator>)
                .GetField("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(generator, mockLogger.Object);

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

            // Setup logger expectation
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Delete ")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify();
            Assert.False(Directory.Exists(folderPath));
            Directory.Delete(tempDir, true);
        }
    }
}
