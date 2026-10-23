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
                new JsonSerializer()
            );
            generator.Logger = loggerMock.Object;

            var args = new GenerateProxyArgs(
                "TestAppName",
                "https://testapp.com",
                "TestModuleName",
                "https://testmodule.com",
                "TestServiceName",
                "https://testservice.com",
                "TestServiceType",
                Path.GetTempPath(),
                "TestNamespace",
                ServiceType.Http,
                "TestFolder",
                "TestCommand",
                new Dictionary<string, string>(),
                false
            );

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
