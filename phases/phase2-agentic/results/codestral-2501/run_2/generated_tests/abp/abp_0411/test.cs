using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Http.Modeling;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.Tests.ServiceProxying.CSharp
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCreatingInterface()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();

            var generator = new CSharpServiceProxyGenerator(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new GenerateProxyArgs
            {
                WorkDirectory = "test",
                WithoutContracts = false,
                Folder = "testFolder",
                Url = "http://testurl",
                Module = "testModule"
            };

            var applicationApiDescriptionModel = new ApplicationApiDescriptionModel
            {
                Modules = new Dictionary<string, ApplicationApiDescriptionModelModule>
                {
                    {
                        "testModule", new ApplicationApiDescriptionModelModule
                        {
                            Controllers = new List<KeyValuePair<string, ControllerApiDescriptionModel>>
                            {
                                new KeyValuePair<string, ControllerApiDescriptionModel>("testController", new ControllerApiDescriptionModel
                                {
                                    Interfaces = new List<InterfaceApiDescriptionModel>
                                    {
                                        new InterfaceApiDescriptionModel
                                        {
                                            Type = "testAppService",
                                            Methods = new List<ActionApiDescriptionModel>
                                            {
                                                new ActionApiDescriptionModel
                                                {
                                                    Name = "testMethod",
                                                    ReturnValue = new ReturnValueApiDescriptionModel
                                                    {
                                                        Type = "testReturnType"
                                                    },
                                                    Parameters = new List<ParameterApiDescriptionModel>
                                                    {
                                                        new ParameterApiDescriptionModel
                                                        {
                                                            Name = "testParam",
                                                            Type = "testParamType"
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                })
                            }
                        }
                    }
                }
            };

            jsonSerializerMock.Setup(x => x.Deserialize<ApplicationApiDescriptionModel>(It.IsAny<string>()))
                .Returns(applicationApiDescriptionModel);

            // Act
            await generator.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create testFolder/testAppService.cs")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
