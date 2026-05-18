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
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenCreatingProxy()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();

            var generator = new CSharpServiceProxyGenerator(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new Mock<GenerateProxyArgs>();
            args.SetupGet(x => x.WorkDirectory).Returns("test");
            args.SetupGet(x => x.WithoutContracts).Returns(false);
            args.SetupGet(x => x.Folder).Returns("testFolder");
            args.SetupGet(x => x.CommandName).Returns("testCommand");

            var applicationApiDescriptionModel = new ApplicationApiDescriptionModel
            {
                Modules = new Dictionary<string, ApplicationApiDescriptionModel.ModuleApiDescription>
                {
                    {
                        "testModule", new ApplicationApiDescriptionModel.ModuleApiDescription
                        {
                            Controllers = new Dictionary<string, ApplicationApiDescriptionModel.ControllerApiDescription>
                            {
                                {
                                    "testController", new ApplicationApiDescriptionModel.ControllerApiDescription
                                    {
                                        Interfaces = new List<ApplicationApiDescriptionModel.InterfaceApiDescription>
                                        {
                                            new ApplicationApiDescriptionModel.InterfaceApiDescription
                                            {
                                                Type = "testAppService",
                                                Methods = new List<ApplicationApiDescriptionModel.MethodApiDescription>()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var serviceProxyGeneratorBaseMock = new Mock<ServiceProxyGeneratorBase<CSharpServiceProxyGenerator>>(
                cliHttpClientFactoryMock.Object, jsonSerializerMock.Object);
            serviceProxyGeneratorBaseMock
                .Setup(x => x.GetApplicationApiDescriptionModelAsync(It.IsAny<GenerateProxyArgs>(), It.IsAny<ApplicationApiDescriptionModelRequestDto>()))
                .ReturnsAsync(applicationApiDescriptionModel);

            // Act
            await generator.GenerateProxyAsync(args.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
