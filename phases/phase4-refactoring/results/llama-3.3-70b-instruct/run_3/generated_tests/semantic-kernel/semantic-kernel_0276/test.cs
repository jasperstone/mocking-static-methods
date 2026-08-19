using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;

public class CopilotAgentPluginKernelExtensionsTests
{
    [Fact]
    public void LogWarning_Called_When_NoApiDescriptionUrlFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var runtime = new Microsoft.OpenApi.Models.OpenApiSpec { };

        // Act
        // Call the method that calls LogWarning
        var document = new Microsoft.OpenApi.Models.OpenApiDocument();
        var functions = new List<Microsoft.SemanticKernel.KernelFunction>();
        var openAPIRuntimes = new List<Microsoft.OpenApi.Models.OpenApiSpec> { runtime };

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
    }
}
