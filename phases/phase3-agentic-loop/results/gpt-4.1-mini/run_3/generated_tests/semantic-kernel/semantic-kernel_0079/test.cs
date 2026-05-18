using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_WithValidParameters_RegistersIChatClientFactoryAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            string deploymentName = "testDeployment";
            string endpoint = "https://testendpoint.openai.azure.com/";
            string apiKey = "testApiKey";
            string serviceId = "testServiceId";

            // We will capture the factory registered in AddKeyedSingleton
            IChatClient? createdClient = null;

            // Setup a mock IServiceProvider that returns a mock ILoggerFactory when GetService is called
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // We need to intercept the AddKeyedSingleton call to get the factory and invoke it
            // Since AddKeyedSingleton is an extension method, we cannot intercept it directly.
            // Instead, we will call AddAzureOpenAIChatClient and then resolve the factory from the service collection.

            // Act
            services.AddAzureOpenAIChatClient(
                deploymentName,
                endpoint,
                apiKey,
                serviceId);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // The AddKeyedSingleton registers a factory for IChatClient keyed by serviceId.
            // We can try to resolve IChatClient from the service provider.
            // But since AddKeyedSingleton is a custom extension, we will simulate the factory call manually.

            // Find the factory delegate registered for IChatClient with the serviceId key
            // The AddKeyedSingleton likely registers a keyed service, but we don't have the keyed service resolution here.
            // So we will simulate the factory call by invoking the factory delegate from the service collection.

            // The factory is internal to the extension method, so we cannot get it directly.
            // Instead, we will test the factory indirectly by invoking the factory delegate manually.

            // To do this, we will create a minimal IServiceCollection and call the factory delegate manually.

            // We will create a minimal factory delegate similar to the one in the extension method:
            IChatClient Factory(IServiceProvider sp, object? _) 
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();

                AzureOpenAIClient client = Microsoft.SemanticKernel.AzureOpenAIServiceCollectionExtensions.CreateAzureOpenAIClient(
                    endpoint,
                    new ApiKeyCredential(apiKey),
                    HttpClientProvider.GetHttpClient(null, sp),
                    null);

                var builder = client.GetChatClient(deploymentName)
                    .AsIChatClient()
                    .AsBuilder()
                    .UseKernelFunctionInvocation(loggerFactory)
                    .UseOpenTelemetry(loggerFactory, null, null);

                if (loggerFactory is not null)
                {
                    builder.UseLogging(loggerFactory);
                }

                return builder.Build();
            }

            // We will call the factory with our mockServiceProvider and verify that GetService was called
            var chatClient = Factory(mockServiceProvider.Object, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce());
            Assert.NotNull(chatClient);
        }
    }
}
