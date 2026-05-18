using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;
using Volo.Abp.IO;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests;

public class CSharpServiceProxyGeneratorTests
{
    [Fact]
    public async Task Should_LogInformation_When_Generating_Interface_File()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var jsonSerializerMock = new Mock<Volo.Abp.Json.IJsonSerializer>();

        var generator = new CSharpServiceProxyGenerator(
            cliHttpClientFactoryMock.Object,
            jsonSerializerMock.Object
        );

        // Set the protected Logger property via reflection
        var loggerProperty = typeof(ServiceProxyGeneratorBase<CSharpServiceProxyGenerator>)
            .GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loggerProperty!.SetValue(generator, loggerMock.Object);

        var args = new GenerateProxyArgs(
            commandName: "generate-proxy",
            workDirectory: "/path/to/workdir",
            module: "TestModule",
            url: "https://localhost:44302",
            output: null,
            target: CSharpServiceProxyGenerator.Name,
            apiName: null,
            source: null,
            folder: "TestFolder",
            serviceType: null,
            entryPoint: null,
            withoutContracts: false
        );

        // Create minimal API model that passes the controller filter
        var controller = new ControllerApiDescriptionModel
        {
            Interfaces = new List<InterfaceApiDescriptionModel>
            {
                new InterfaceApiDescriptionModel
                {
                    Type = "TestAppService", // Ends with AppService to pass filter
                    Methods = new List<ActionApiDescriptionModel>()
                }
            }
        };

        var module = new ModuleApiDescriptionModel
        {
            Controllers = new Dictionary<string, ControllerApiDescriptionModel>
            {
                ["TestController"] = controller
            }
        };

        var apiModel = ApplicationApiDescriptionModel.Create();
        apiModel.AddModule(module);

        // Mock HTTP client to avoid real calls
        var mockHttpClient = new Mock<System.Net.Http.HttpClient>();
        cliHttpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<bool>()))
            .Returns(mockHttpClient.Object);
        
        mockHttpClient.Setup(x => x.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync("{}");
        
        jsonSerializerMock.Setup(x => x.Deserialize<ApplicationApiDescriptionModel>(It.IsAny<string>()))
            .Returns(apiModel);

        // Act
        await generator.GenerateProxyAsync(args);

        // Assert - verify the LogInformation call for interface file creation (line 264)
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.StartsWith("Create ") && msg.Contains("TestFolder") && msg.Contains(".cs")),
                It.IsAny<object[]>()
            ),
            Times.AtLeastOnce
        );
    }
}
