using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ServiceProxying.CSharp;

namespace Volo.Abp.Cli.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock;
        private readonly CSharpServiceProxyGenerator _generator;

        public CSharpServiceProxyGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            _generator = new CSharpServiceProxyGenerator(
                Mock.Of<CLI.Http.CliHttpClientFactory>(),
                Mock.Of<IJsonSerializer>());
            _generator.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var args = new GenerateProxyArgs
            {
                WorkDirectory = Path.GetTempPath(),
                Folder = "TestFolder",
                CommandName = "SomeCommand",
                WithoutContracts = false
            };

            // Act
            await _generator.GenerateProxyAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.StartsWith("Create") || s.StartsWith("Delete"))),
                Times.AtLeastOnce);
        }
    }
}
