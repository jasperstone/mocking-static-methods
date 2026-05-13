using System;
using System.Collections.Generic;
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
        private readonly Mock<CSharpServiceProxyGenerator> _generatorMock;

        public CSharpServiceProxyGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            _generatorMock = new Mock<CSharpServiceProxyGenerator>(Mock.Of<CLIHttpClientFactory>(), Mock.Of<IJsonSerializer>())
            {
                CallBase = true
            };
            _generatorMock.Setup(g => g.Logger).Returns(_loggerMock.Object);
        }

        [Fact]
        public async Task GenerateProxyAsync_Should_LogInformation_When_CommandIsRemoveProxy()
        {
            // Arrange
            var args = new GenerateProxyArgs
            {
                CommandName = RemoveProxyCommand.Name,
                WorkDirectory = "testDir",
                Folder = "testFolder"
            };

            var folderPath = Path.Combine(args.WorkDirectory, args.Folder);
            Directory.CreateDirectory(folderPath);

            _generatorMock.Setup(g => g.GetLoggerOutputPath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string path, string workDir) => $"OutputPath: {path}");

            // Act
            await _generatorMock.Object.GenerateProxyAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains($"Delete {folderPath}"))),
                Times.Once);

            // Cleanup
            Directory.Delete(folderPath, true);
        }
    }
}
