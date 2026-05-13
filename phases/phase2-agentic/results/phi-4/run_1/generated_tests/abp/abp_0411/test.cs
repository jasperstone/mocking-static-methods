using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ServiceProxying.CSharp;

public class CSharpServiceProxyGeneratorTests
{
    [Fact]
    public async Task GenerateProxyAsync_LogsInformationMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
        var generator = new CSharpServiceProxyGenerator(
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<IJsonSerializer>())
        {
            Logger = loggerMock.Object
        };

        var args = new GenerateProxyArgs
        {
            WorkDirectory = "test_work_directory",
            Folder = "test_folder",
            WithoutContracts = false
        };

        // Act
        await generator.GenerateProxyAsync(args);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("Create ")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
