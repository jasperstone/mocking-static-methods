using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Http.Modeling;
using Xunit;

namespace Volo.Abp.Cli.Tests.ServiceProxying.CSharp
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCreatingInterface()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();

            var args = new GenerateProxyArgs(
                "generate-proxy",
                Directory.GetCurrentDirectory(),
                "TestModule",
                "http://testurl",
                "output",
                "csharp",
                "apiName",
                "source",
                "folder",
                ServiceType.Application,
                "entryPoint",
                false,
                new Dictionary<string, string>()
            );

            var generator = new CSharpServiceProxyGenerator(cliHttpClientFactoryFactoryMock.Object, jsonSerializerMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>()),
                Times.AtLeastOnce);
        }
    }
}
