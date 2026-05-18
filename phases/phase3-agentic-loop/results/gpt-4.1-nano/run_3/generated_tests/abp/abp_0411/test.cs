using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ServiceProxying.CSharp;

namespace Volo.Abp.Cli.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock;
        private readonly Mock<CSharpServiceProxyGenerator> _generatorMock;

        public CSharpServiceProxyGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            _generatorMock = new Mock<CSharpServiceProxyGenerator>(Mock.Of<CliHttpClientFactory>(), Mock.Of<IJsonSerializer>())
            {
                CallBase = true
            };
            _generatorMock.Setup(g => g.Logger).Returns(_loggerMock.Object);
        }

        [Fact]
        public async void GenerateProxyAsync_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var args = new GenerateProxyArgs
            {
                WorkDirectory = "testDir",
                Folder = "testFolder",
                CommandName = "generate"
            };

            var folderPath = Path.Combine(args.WorkDirectory, args.Folder);
            Directory.CreateDirectory(folderPath);

            // Act
            await _generatorMock.Object.GenerateProxyAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains($"Delete {folderPath}"))),
                Times.Never);
        }
    }
}
