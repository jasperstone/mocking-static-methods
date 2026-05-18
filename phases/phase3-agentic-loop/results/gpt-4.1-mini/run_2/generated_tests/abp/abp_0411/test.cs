using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.ServiceProxying;
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
            var mockCliHttpClientFactory = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>(MockBehavior.Strict, null);
            var mockJsonSerializer = new Mock<Volo.Abp.Json.IJsonSerializer>(MockBehavior.Strict);

            var generator = new CSharpServiceProxyGenerator(mockCliHttpClientFactory.Object, mockJsonSerializer.Object);

            // Inject the mock logger into the base class Logger property via reflection
            var loggerProperty = typeof(ServiceProxyGeneratorBase<CSharpServiceProxyGenerator>)
                .GetProperty("Logger", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (loggerProperty != null && loggerProperty.CanWrite)
            {
                loggerProperty.SetValue(generator, mockLogger.Object);
            }
            else
            {
                var loggerField = typeof(ServiceProxyGeneratorBase<CSharpServiceProxyGenerator>)
                    .GetField("_logger", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (loggerField != null)
                {
                    loggerField.SetValue(generator, mockLogger.Object);
                }
            }

            var args = new GenerateProxyArgs(
                RemoveProxyCommand.Name,
                Path.GetTempPath(),
                module: null,
                url: null,
                output: null,
                target: null,
                apiName: null,
                source: null,
                folder: null,
                serviceType: null,
                entryPoint: null,
                withoutContracts: false);

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Delete ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
