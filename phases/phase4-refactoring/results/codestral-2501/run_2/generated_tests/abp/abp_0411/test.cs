using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying.CSharp;
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
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();

            var args = new GenerateProxyArgs
            {
                WorkDirectory = "test",
                WithoutContracts = false,
                Folder = "testFolder",
                CommandName = "testCommand"
            };

            var generator = new Mock<CSharpServiceProxyGenerator>(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object, args)
            {
                CallBase = true
            };

            generator.Setup(x => x.Logger).Returns(loggerMock.Object);
            generator.Setup(x => x.GetDefaultServiceType(It.IsAny<GenerateProxyArgs>())).Returns("testServiceType");

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
                                                Type = "testAppService"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            cliHttpClientFactoryMock.Setup(x => x.GetApplicationApiDescriptionModelAsync(It.IsAny<GenerateProxyArgs>(), It.IsAny<ApplicationApiDescriptionModelRequestDto>()))
                .ReturnsAsync(applicationApiDescriptionModel);

            // Act
            await generator.Object.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
