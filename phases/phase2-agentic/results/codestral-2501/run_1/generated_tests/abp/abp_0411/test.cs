using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Http.Modeling;
using Volo.Abp.IO;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly CSharpServiceProxyGenerator _generator;

        public CSharpServiceProxyGeneratorTests()
        {
            _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _generator = new CSharpServiceProxyGenerator(_cliHttpClientFactoryMock.Object, _jsonSerializerMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCreatingInterface()
        {
            // Arrange
            var args = new GenerateProxyArgs
            {
                WorkDirectory = "workDir",
                Folder = "folder",
                WithoutContracts = false
            };
            var applicationApiDescriptionModel = new ApplicationApiDescriptionModel
            {
                Modules = new Dictionary<string, ApplicationApiDescriptionModel.Module>
                {
                    {
                        "module", new ApplicationApiDescriptionModel.Module
                        {
                            Controllers = new List<KeyValuePair<string, ControllerApiDescriptionModel>>
                            {
                                new KeyValuePair<string, ControllerApiDescriptionModel>("controller", new ControllerApiDescriptionModel
                                {
                                    Interfaces = new List<InterfaceApiDescriptionModel>
                                    {
                                        new InterfaceApiDescriptionModel
                                        {
                                            Type = "AppService",
                                            Methods = new List<MethodApiDescriptionModel>()
                                        }
                                    }
                                })
                            }
                        }
                    }
                }
            };
            _jsonSerializerMock.Setup(x => x.Deserialize<ApplicationApiDescriptionModel>(It.IsAny<string>()))
                .Returns(applicationApiDescriptionModel);

            // Act
            await _generator.GenerateProxyAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
