using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        private class TestCSharpServiceProxyGenerator : CSharpServiceProxyGenerator
        {
            public TestCSharpServiceProxyGenerator() : base(null!, null!)
            {
                // Override Logger to allow injection
                Logger = _loggerMock.Object;
            }

            public Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();

            public new ILogger<CSharpServiceProxyGenerator> Logger { get; set; }
        }

        [Fact]
        public async Task GenerateProxyAsync_LogsInformationOnRemoveProxyCommand()
        {
            // Arrange
            var generator = new TestCSharpServiceProxyGenerator();

            var args = new GenerateProxyArgs(
                RemoveProxyCommand.Name,
                Path.GetTempPath(),
                null,
                null,
                null,
                null,
                null,
                null,
                "ClientProxies",
                null,
                null,
                false);

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            generator._loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Delete")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
