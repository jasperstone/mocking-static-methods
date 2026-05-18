using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ServiceProxying.CSharp;

public class CSharpServiceProxyGeneratorTests
{
    private readonly Mock<ILogger<CSharpServiceProxyGenerator>> _loggerMock;
    private readonly CSharpServiceProxyGenerator _generator;

    public CSharpServiceProxyGeneratorTests()
    {
        _loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
        _generator = new CSharpServiceProxyGenerator(null, null)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task GenerateProxyAsync_LogsInformationMessage()
    {
        // Arrange
        var args = new GenerateProxyArgs
        {
            WorkDirectory = "test_work_directory",
            Folder = "test_folder",
            WithoutContracts = false
        };

        // Act
        await _generator.GenerateProxyAsync(args);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogInformation(It.Is<string>(s => s.Contains("Create"))),
            Times.Once);
    }
}
