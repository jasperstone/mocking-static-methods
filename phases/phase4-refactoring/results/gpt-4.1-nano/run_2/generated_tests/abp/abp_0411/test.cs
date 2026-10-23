using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    // Minimal stubs for production types
    public class CliHttpClientFactory { }
    public interface IJsonSerializer { }
    public class GenerateProxyArgs
    {
        public string WorkDirectory { get; set; }
        public string Folder { get; set; }
        public string CommandName { get; set; }
        public bool WithoutContracts { get; set; }
    }

    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCreatingFile()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var generator = new CSharpServiceProxyGenerator(
                new CliHttpClientFactory(),
                new Mock<IJsonSerializer>().Object);
            generator.Logger = mockLogger.Object;

            var args = new GenerateProxyArgs
            {
                WorkDirectory = Path.GetTempPath(),
                Folder = "TestFolder",
                CommandName = "Generate",
                WithoutContracts = false
            };

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(msg => msg.StartsWith("Create "))),
                Times.AtLeastOnce);
        }
    }
}
