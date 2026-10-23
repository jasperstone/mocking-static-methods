using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.Tests;

public class CSharpServiceProxyGeneratorTests
{
    private readonly Mock<CliHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IJsonSerializer> _mockJsonSerializer;
    private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _mockLogger;
    private readonly CSharpServiceProxyGenerator _generator;

    public CSharpServiceProxyGeneratorTests()
    {
        _mockHttpClientFactory = new Mock<CliHttpClientFactory>();
        _mockJsonSerializer = new Mock<IJsonSerializer>();
        _mockLogger = new Mock<ILogger<CSharpServiceProxyGenerator>>();

        _generator = new CSharpServiceProxyGenerator(
            _mockHttpClientFactory.Object,
            _mockJsonSerializer.Object);

        _generator.Logger = _mockLogger.Object;
    }

    [Fact]
    public async Task GenerateProxyAsync_Should_LogInformation_For_Interface_File_Creation()
    {
        // Arrange
        var args = new GenerateProxyArgs(
            commandName: "generate-proxy",
            workDirectory: "/tmp/test",
            module: "default",
            url: "https://localhost",
            output: null,
            target: CSharpServiceProxyGenerator.Name,
            apiName: null,
            source: null,
            folder: "ClientProxies",
            serviceType: null,
            entryPoint: null,
            withoutContracts: false);

        // Minimal mock API model that passes the filter: has Interfaces and ends with "AppService"
        var apiModel = new ApplicationApiDescriptionModel
        {
            Modules = new Dictionary<string, ModuleApiDescriptionModel>
            {
                ["default"] = new ModuleApiDescriptionModel
                {
                    Controllers = new Dictionary<string, ControllerApiDescriptionModel>
                    {
                        ["TestController"] = new ControllerApiDescriptionModel
                        {
                            Interfaces = new List<ControllerInterfaceApiDescriptionModel>
                            {
                                new ControllerInterfaceApiDescriptionModel
                                {
                                    Type = "TestAppService", // Triggers ServicePostfixes.Any() check
                                    Methods = new List<ActionApiDescriptionModel>()
                                }
                            }
                        }
                    }
                }
            }
        };

        var mockClient = new Mock<HttpClient>();
        mockClient.Setup(x => x.GetStringAsync(It.IsAny<string>()))
                 .ReturnsAsync("{\"some\":\"json\"}");
        
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<bool>()))
                             .Returns(mockClient.Object);
        _mockJsonSerializer.Setup(x => x.Deserialize<ApplicationApiDescriptionModel>(It.IsAny<string>()))
                          .Returns(apiModel);

        // Act
        await _generator.GenerateProxyAsync(args);

        // Assert - Verify LogInformation extension call (line 264) for interface file creation
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).StartsWith("Create ") && ((string)v).Contains(".cs")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
