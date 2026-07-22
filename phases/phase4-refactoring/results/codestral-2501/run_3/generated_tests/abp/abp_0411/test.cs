using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Http.Modeling;
using Volo.Abp.Json;
using Xunit;

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

        var args = new GenerateProxyArgs(
            "GenerateProxy",
            "WorkDirectory",
            "Module",
            "Url",
            "Output",
            "Target",
            "ApiName",
            "Source",
            "Folder",
            ServiceType.Application,
            "EntryPoint",
            false,
            new Dictionary<string, string>()
        );

        // Act
        await generator.GenerateProxyAsync(args);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
