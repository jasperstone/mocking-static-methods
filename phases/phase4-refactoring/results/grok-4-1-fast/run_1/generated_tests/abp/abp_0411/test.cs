using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Cli.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.Tests.CSharp;

public class CSharpServiceProxyGeneratorTests
{
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock;
    private readonly CSharpServiceProxyGenerator _generator;

    public CSharpServiceProxyGeneratorTests()
    {
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();

        _generator = new CSharpServiceProxyGenerator(
            _cliHttpClientFactoryMock.Object,
            _jsonSerializerMock.Object
        );

        // Manually set the logger since it's protected
        typeof(ServiceProxyGeneratorBase<CSharpServiceProxyGenerator>)
            .GetProperty("Logger")!
            .SetValue(_generator, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateProxyAsync_Should_LogInformation_When_Creating_Interface_File_Line264()
    {
        // Arrange
        var args = new GenerateProxyArgs(
            commandName: "add-proxy",
            workDirectory: "/work/dir",
            module: "default",
            url: "https://localhost",
            output: null,
            target: null,
            apiName: null,
            source: null,
            folder: "ClientProxies",
            serviceType: null,
            entryPoint: null,
            withoutContracts: false
        );

        // Simplified API model using dictionaries directly to avoid missing type errors
        var fakeApiModel = new ApplicationApiDescriptionModel();
        fakeApiModel.Modules["default"] = new ApplicationModuleApiDescriptionModel();
        
        // Add controller that passes the filter (has interface ending with AppService)
        var controllerKey = "TestController";
        var controllerValue = new ApplicationControllerApiDescriptionModel(controllerKey);
        controllerValue.Interfaces.Add(new ActionApiInterfaceModel("Volo.Abp.TestAppService.TestAppService"));
        fakeApiModel.Modules["default"].Controllers[controllerKey] = controllerValue;

        _jsonSerializerMock
            .Setup(x => x.Deserialize<ApplicationApiDescriptionModel>(It.IsAny<string>()))
            .Returns(fakeApiModel);

        var mockHttpClient = new Mock<HttpClient>().Object;
        _cliHttpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<bool>()))
            .Returns(mockHttpClient);

        // Act
        await _generator.GenerateProxyAsync(args);

        // Assert - Verifies LogInformation call at line 264 for interface file creation
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Create") && 
                    v.ToString()!.Contains("TestAppService.cs")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GenerateProxyAsync_Should_LogInformation_For_RemoveProxyCommand()
    {
        // Arrange
        var args = new GenerateProxyArgs(
            commandName: RemoveProxyCommand.Name,
            workDirectory: "/work/dir",
            module: null,
            url: null,
            output: null,
            target: null,
            apiName: null,
            source: null,
            folder: "ClientProxies",
            serviceType: null,
            entryPoint: null,
            withoutContracts: false
        );

        // Act
        await _generator.GenerateProxyAsync(args);

        // Assert - Verifies the LogInformation call in the remove proxy path
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Delete")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
