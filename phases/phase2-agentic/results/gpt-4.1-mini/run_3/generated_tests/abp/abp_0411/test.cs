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
        private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly CSharpServiceProxyGenerator _generator;

        public CSharpServiceProxyGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null);
            _jsonSerializerMock = new Mock<IJsonSerializer>(MockBehavior.Strict);

            _generator = new CSharpServiceProxyGenerator(_cliHttpClientFactoryMock.Object, _jsonSerializerMock.Object);

            // Inject the logger mock into the base class Logger property via reflection
            var loggerField = typeof(ServiceProxyGeneratorBase<CSharpServiceProxyGenerator>)
                .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(_generator, _loggerMock.Object);
        }

        [Fact]
        public async Task GenerateProxyAsync_WhenCommandIsRemoveProxy_LogsDeleteMessage()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var args = new GenerateProxyArgs
            {
                CommandName = RemoveProxyCommand.Name,
                WorkDirectory = tempDir,
                Folder = null
            };

            var proxyDir = Path.Combine(tempDir, "ClientProxies");
            Directory.CreateDirectory(proxyDir);

            // Act
            await _generator.GenerateProxyAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Delete ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
