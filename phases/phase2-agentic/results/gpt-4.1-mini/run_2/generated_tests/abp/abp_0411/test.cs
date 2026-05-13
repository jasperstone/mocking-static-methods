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
                .GetProperty("Logger", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (loggerField != null)
            {
                loggerField.SetValue(_generator, _loggerMock.Object);
            }
            else
            {
                // fallback: try to set the field named Logger
                var loggerField2 = typeof(ServiceProxyGeneratorBase<CSharpServiceProxyGenerator>)
                    .GetField("_logger", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (loggerField2 != null)
                {
                    loggerField2.SetValue(_generator, _loggerMock.Object);
                }
            }
        }

        [Fact]
        public async Task GenerateProxyAsync_WhenCommandIsRemoveProxy_LogsDeleteMessage()
        {
            // Arrange
            var workDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(workDir);
            var folder = "TestFolder";
            var folderPath = Path.Combine(workDir, folder);
            Directory.CreateDirectory(folderPath);

            var args = new GenerateProxyArgs
            {
                CommandName = RemoveProxyCommand.Name,
                WorkDirectory = workDir,
                Folder = folder
            };

            // Act
            await _generator.GenerateProxyAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Delete")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }
        }
    }
}
