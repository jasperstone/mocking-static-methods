using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Mock<AbpCliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var generateProxyArgs = new GenerateProxyArgs
            {
                WorkDirectory = "workDirectory",
                Folder = "folder"
            };

            var csharpServiceProxyGenerator = new CSharpServiceProxyGenerator(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object);
            csharpServiceProxyGenerator.Logger = loggerMock.Object;

            // Act
            await csharpServiceProxyGenerator.GenerateProxyAsync(generateProxyArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
