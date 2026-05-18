using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_LogsInformation_WhenCreatingFile()
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

            var args = new GenerateProxyArgs(
                commandName: "GenerateProxy",
                workDirectory: Path.GetTempPath(),
                module: null,
                url: null,
                output: null,
                target: null,
                apiName: null,
                source: null,
                folder: "TestFolder",
                serviceType: null,
                entryPoint: null,
                withoutContracts: false);

            // Setup the mockLogger to verify LogInformation calls
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create ")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act & Assert
            try
            {
                await generator.GenerateProxyAsync(args);
            }
            catch
            {
                // Ignored: we only want to verify logging call
            }

            mockLogger.Verify();
        }
    }
}
