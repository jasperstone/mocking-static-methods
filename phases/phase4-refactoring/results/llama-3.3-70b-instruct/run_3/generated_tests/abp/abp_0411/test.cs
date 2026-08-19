using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Cli.Core.Tests
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
            var generator = new CSharpServiceProxyGenerator(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object);
            var args = new GenerateProxyArgs { WorkDirectory = "workDirectory", Folder = "folder" };

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
