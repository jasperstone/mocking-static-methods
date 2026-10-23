using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Azure.Core;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace SemanticKernel.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_WithServiceProvider_ShouldResolveILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var builderMock = new Mock<IKernelBuilder>();
            builderMock.Setup(b => b.Services).Returns(services);
            var builder = builderMock.Object;

            // Act
            builder.AddAzureOpenAIChatClient(
                deploymentName: "test-deployment",
                endpoint: "https://test.openai.azure.com",
                apiKey: "test-api-key");

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ILoggerFactory>();
            Assert.NotNull(service);
        }
    }
}
