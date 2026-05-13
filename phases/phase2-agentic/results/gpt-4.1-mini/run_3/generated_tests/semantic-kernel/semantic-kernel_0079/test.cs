using System;
using System.Net.Http;
using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_WithApiKey_RegistersIChatClientFactory_AndCallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object)
                .Verifiable();

            // Add a dummy HttpClient to the services to avoid null issues
            services.AddSingleton<HttpClient>();

            // Act
            services.AddAzureOpenAIChatClient(
                deploymentName: "testDeployment",
                endpoint: "https://test.endpoint",
                apiKey: "testApiKey");

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the IChatClient factory delegate registered with the service collection
            var factoryDescriptor = services
                .SingleOrDefault(sd => sd.ServiceType == typeof(IChatClient) && sd.ImplementationFactory != null);

            Assert.NotNull(factoryDescriptor);

            var factory = factoryDescriptor.ImplementationFactory;

            // Call the factory with the mocked service provider to trigger the GetService call
            var chatClient = factory(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
            Assert.NotNull(chatClient);
        }
    }
}
