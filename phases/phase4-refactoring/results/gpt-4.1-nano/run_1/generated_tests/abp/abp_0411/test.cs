using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ServiceProxying.CSharp;

namespace Volo.Abp.Cli.Tests
{
    // Subclass to expose Logger property for testing
    public class TestCSharpServiceProxyGenerator : CSharpServiceProxyGenerator
    {
        public ILogger<CSharpServiceProxyGenerator> Logger { get; set; }

        public TestCSharpServiceProxyGenerator(ILogger<CSharpServiceProxyGenerator> logger, CliHttpClientFactory httpClientFactory, IJsonSerializer jsonSerializer)
            : base(httpClientFactory, jsonSerializer)
        {
            Logger = logger;
        }

        // Override method to use injected logger
        protected override void LogInformation(string message)
        {
            Logger.LogInformation(message);
        }
    }

    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_Should_LogInformation_When_CreatingFile()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockJsonSerializer = new Mock<IJsonSerializer>();

            var generator = new TestCSharpServiceProxyGenerator(mockLogger.Object, mockHttpClientFactory.Object, mockJsonSerializer.Object);

            var args = new GenerateProxyArgs
            {
                WorkDirectory = Path.GetTempPath(),
                Folder = "TestFolder",
                CommandName = "Generate",
                WithoutContracts = false
            };

            // Mock the parts of GenerateProxyAsync to reach the logging call
            // For simplicity, assume the method will reach the point of logging
            // and that the directory exists or is created

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(msg => msg.Contains("Create"))),
                Times.AtLeastOnce);
        }
    }
}
