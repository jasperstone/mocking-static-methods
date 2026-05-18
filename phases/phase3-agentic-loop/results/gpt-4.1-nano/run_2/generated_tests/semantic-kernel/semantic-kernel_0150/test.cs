using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_Should_Call_GetService_For_LoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Act
            services.AddOllamaChatCompletion("modelId", new Uri("http://localhost"), "serviceId");
            var serviceProvider = services.BuildServiceProvider();

            // Use reflection to get the method info
            var methodInfo = typeof(Microsoft.SemanticKernel.OllamaServiceCollectionExtensions)
                .GetMethod("AddOllamaChatCompletion", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(IServiceCollection), typeof(string), typeof(Uri), typeof(string) }, null);
            Assert.NotNull(methodInfo);

            // Invoke the method to get the delegate
            var delegateObj = methodInfo.Invoke(null, new object[] { services, "modelId", new Uri("http://localhost"), "serviceId" });
            Assert.NotNull(delegateObj);

            // The method returns IServiceCollection, so cast
            var resultServices = delegateObj as IServiceCollection;
            Assert.NotNull(resultServices);

            // Build provider and verify
            var provider = resultServices.BuildServiceProvider();

            // Use reflection to get the private lambda inside the method
            // Since it's an extension method, we need to invoke the method directly
            // But for simplicity, we can test the core logic separately
            // Alternatively, test the lambda directly if accessible
            // Here, we test the core logic separately

            // Create a mock for IServiceProvider
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Call the lambda directly
            var builder = new OllamaApiClient(new Uri("http://localhost"), "modelId").AsBuilder();

            // Use reflection to get the method info for the extension method
            var extensionMethod = typeof(OllamaServiceCollectionExtensions)
                .GetMethod("AddOllamaChatCompletion", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(IServiceCollection), typeof(string), typeof(Uri), typeof(string) }, null);
            Assert.NotNull(extensionMethod);

            // Call the extension method
            var servicesResult = (IServiceCollection)extensionMethod.Invoke(null, new object[] { services, "modelId", new Uri("http://localhost"), "serviceId" });
            Assert.NotNull(servicesResult);

            // Build provider
            var sp = servicesResult.BuildServiceProvider();

            // Now, test the core logic: get the ILoggerFactory
            var loggerFactory = sp.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);

            // Verify that GetService was called
            mockLoggerFactory.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
