using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Connectors.AzureOpenAI.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_Should_Call_GetService_And_Return_IChatClient()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Register the service provider mock as a singleton
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddAzureOpenAIChatClient(
                deploymentName: "testDeployment",
                endpoint: "https://testendpoint",
                apiKey: "testApiKey",
                serviceId: "testService",
                modelId: "testModel",
                apiVersion: null,
                httpClient: null);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the registered factory
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(Func<IServiceProvider, IChatClient>));
            Assert.NotNull(descriptor);

            // Extract the factory
            var factory = (Func<IServiceProvider, IChatClient>)descriptor.ImplementationInstance
                ?? (Func<IServiceProvider, IChatClient>)descriptor.ImplementationFactory?.Invoke(provider);

            // Call the factory
            var chatClient = factory(provider);

            // Assert
            Assert.NotNull(chatClient);
            Assert.IsAssignableFrom<IChatClient>(chatClient);
            // Verify that GetService was called
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
