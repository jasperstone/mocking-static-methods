using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using System.IO;

namespace Volo.Abp.Cli.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock;
        private readonly CSharpServiceProxyGenerator _generator;

        public CSharpServiceProxyGeneratorTests()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Moq.Mock<IVolo.Abp.Cli.Http.CliHttpClientFactory>();
            var jsonSerializerMock = new Moq.Mock<IVolo.Abp.Json.IJsonSerializer>();
            _generator = new CSharpServiceProxyGenerator(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCreatingFile()
        {
            // Arrange
            var args = new GenerateProxyArgs
            {
                WorkDirectory = "testDir",
                Folder = "subFolder",
                CommandName = "generate",
                WithoutContracts = false
            };

            // Mock GetApplicationApiDescriptionModelAsync to do nothing
            var generatorMock = new Moq.Mock<CSharpServiceProxyGenerator>(null, null);
            generatorMock.CallBase = true;
            generatorMock.Setup(g => g.GetApplicationApiDescriptionModelAsync(It.IsAny<GenerateProxyArgs>(), It.IsAny<ApplicationApiDescriptionModelRequestDto>()))
                .ReturnsAsync(new ApplicationApiDescriptionModel());

            // Act
            await generatorMock.Object.GenerateProxyAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.StartsWith("Create "))),
                Times.AtLeastOnce);
        }
    }
}
