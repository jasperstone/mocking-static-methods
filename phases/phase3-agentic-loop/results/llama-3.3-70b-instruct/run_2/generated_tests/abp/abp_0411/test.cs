using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;
using Volo.Abp.IO;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_LogInformation_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Mock<ICliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var generateProxyArgs = new GenerateProxyArgs(
                workDirectory: "workDirectory",
                folder: "folder",
                commandName: "commandName",
                withoutContracts: false
            );

            var cSharpServiceProxyGenerator = new CSharpServiceProxyGenerator(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object);
            cSharpServiceProxyGenerator.Logger = loggerMock.Object;

            // Act
            await cSharpServiceProxyGenerator.GenerateProxyAsync(generateProxyArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GenerateProxyAsync_LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Mock<ICliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var generateProxyArgs = new GenerateProxyArgs(
                workDirectory: "workDirectory",
                folder: "folder",
                commandName: "commandName",
                withoutContracts: false
            );

            var cSharpServiceProxyGenerator = new CSharpServiceProxyGenerator(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object);
            cSharpServiceProxyGenerator.Logger = loggerMock.Object;

            // Act
            await cSharpServiceProxyGenerator.GenerateProxyAsync(generateProxyArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.StartsWith("Create "))), Times.AtLeastOnce);
        }
    }
}
