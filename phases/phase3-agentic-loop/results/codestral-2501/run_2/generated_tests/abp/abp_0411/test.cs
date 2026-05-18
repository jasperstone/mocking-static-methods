using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;
using Volo.Abp.IO;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenProxyIsGenerated()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockJsonSerializer = new Mock<IJsonSerializer>();

            var generator = new CSharpServiceProxyGenerator(mockCliHttpClientFactory.Object, mockJsonSerializer.Object)
            {
                Logger = mockLogger.Object
            };

            var args = new GenerateProxyArgs(
                "test",
                "test",
                "test",
                "test",
                "test",
                "test",
                "test",
                "test",
                "test",
                null,
                "test",
                false,
                new Dictionary<string, string>()
            );

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
