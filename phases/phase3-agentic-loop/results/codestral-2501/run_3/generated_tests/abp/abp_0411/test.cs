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
        public async Task GenerateProxyAsync_ShouldLogInformation_WhenGeneratingProxy()
        {
            // Arrange
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();

            var generator = new CSharpServiceProxyGenerator(cliHttpClientFactoryMock.Object, jsonSerializerMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new GenerateProxyArgs(
                "test",
                "test",
                "test",
                "test",
                "test",
                "test",
                "test",
                "test",
                "test",
                ServiceType.Application,
                "test",
                false,
                new Dictionary<string, string>());

            var applicationApiDescriptionModel = new ApplicationApiDescriptionModel
            {
                Modules = new Dictionary<string, ApplicationApiDescriptionModelModule>
                {
                    {
                        "test", new ApplicationApiDescriptionModelModule
                        {
                            Controllers = new Dictionary<string, ControllerApiDescriptionModel>
                            {
                                {
                                    "test", new ControllerApiDescriptionModel
                                    {
                                        Interfaces = new List<InterfaceApiDescriptionModel>
                                        {
                                            new InterfaceApiDescriptionModel
                                            {
                                                Type = "TestAppService",
                                                Methods = new List<MethodApiDescriptionModel>()
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
            await generator.GenerateProxyAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Exactly(2));
        }
    }
}
