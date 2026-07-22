using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public async Task AddVertexAIGeminiChatCompletion_ServiceProviderGetServiceCalled()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

        var builder = new KernelBuilder();
        var modelId = "modelId";
        var bearerTokenProvider = () => new ValueTask<string>("token");
        var location = "location";
        var projectId = "projectId";

        // Act
        builder.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
    }
}
