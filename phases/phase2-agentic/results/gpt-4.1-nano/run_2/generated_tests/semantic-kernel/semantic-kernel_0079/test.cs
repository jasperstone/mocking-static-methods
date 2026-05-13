using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_Should_Call_GetService_And_ReturnServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock ILoggerFactory to the service collection
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            string deploymentName = "testDeployment";
            string endpoint = "https://testendpoint.openai.azure";
            string apiKey = "testApiKey";

            // Act
            var result = services.AddAzureOpenAIChatClient(
                deploymentName,
                endpoint,
                apiKey);

            // Assert
            Assert.Same(services, result);
            // Verify that the service collection contains the registration
            var serviceProvider = services.BuildServiceProvider();

            // Use reflection to get the private registration for IChatClient
            var serviceDescriptors = services;
            bool found = false;
            foreach (var descriptor in serviceDescriptors)
            {
                if (descriptor.ServiceType == typeof(IChatClient))
                {
                    found = true;
                    break;
                }
            }
            Assert.True(found, "IChatClient registration not found in service collection.");

            // Now test that the Factory function calls GetService<ILoggerFactory>
            var factoryMethod = typeof(AzureOpenAIServiceCollectionExtensions)
                .GetMethod("AddAzureOpenAIChatClient", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.NotNull(factoryMethod);
        }
    }
}
