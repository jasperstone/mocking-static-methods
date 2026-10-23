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
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;
using Volo.Abp.IO;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests
{
    public class CSharpServiceProxyGeneratorTests
    {
        [Fact]
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCreatingInterface()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactory = new Mock<CliHttpClientFactory>().Object;
            var jsonSerializer = new Mock<IJsonSerializer>().Object;

            var generator = new CSharpServiceProxyGenerator(cliHttpClientFactory, jsonSerializer)
            {
                Logger = mockLogger.Object
            };

            var args = new GenerateProxyArgs
            {
                WorkDirectory = "test",
                WithoutContracts = false,
                Folder = "testFolder",
                CommandName = "testCommand"
            };

            var applicationApiDescriptionModel = new ApplicationApiDescriptionModel
            {
                Modules = new Dictionary<string, ApplicationApiDescriptionModel.Module>
                {
                    {
                        "testModule", new ApplicationApiDescriptionModel.Module
                        {
                            Controllers = new Dictionary<string, ControllerApiDescriptionModel>
                            {
                                {
                                    "testController", new ControllerApiDescriptionModel
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
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var mockGenerator = new Mock<CSharpServiceProxyGenerator>(cliHttpClientFactory, jsonSerializer);
            mockGenerator.Setup(x => x.GetApplicationApiDescriptionModelAsync(It.IsAny<GenerateProxyArgs>(), It.IsAny<ApplicationApiDescriptionModelRequestDto>()))
                .ReturnsAsync(applicationApiDescriptionModel);

            // Act
            await mockGenerator.Object.GenerateProxyAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
