using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var generator = new CSharpServiceProxyGenerator(
                new CliHttpClientFactory(),
                new JsonSerializationHelper()
            );
            generator.Logger = loggerMock.Object;

            var args = new GenerateProxyArgs(
                "TestProjectName",
                "TestProjectPath",
                "TestModule",
                "TestController",
                "TestAction",
                "TestHttpMethod",
                "TestRoute",
                "TestAreaName",
                "TestServiceName",
                ServiceType.HttpApi,
                "TestServiceInterface",
                false,
                new Dictionary<string, string>()
            );

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.AtLeastOnce
            );
        }
    }
}
