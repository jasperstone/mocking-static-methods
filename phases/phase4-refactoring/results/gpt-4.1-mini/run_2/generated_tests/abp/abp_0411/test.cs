using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        private class TestCSharpServiceProxyGenerator : CSharpServiceProxyGenerator
        {
            public TestCSharpServiceProxyGenerator() : base(null, null)
            {
            }

            public async Task CallLogInformationAsync(ILogger<CSharpServiceProxyGenerator> logger)
            {
                Logger = logger;
                Logger.LogInformation("Create test path");
                await Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Logger_LogInformation_IsCalledWithCreateMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var generator = new TestCSharpServiceProxyGenerator();

            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()))
                .Verifiable();

            // Act
            await generator.CallLogInformationAsync(loggerMock.Object);

            // Assert
            loggerMock.Verify();
        }
    }
}
