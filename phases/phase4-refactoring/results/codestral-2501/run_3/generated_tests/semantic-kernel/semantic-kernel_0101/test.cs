using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersService()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var httpClient = new HttpClient();

            var serviceCollection = new ServiceCollection();
            kernelBuilderMock.Setup(kb => kb.Services).Returns(serviceCollection);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project-id";
            var serviceId = "test-service-id";

            // Act
            var result = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                kernelBuilderMock.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                serviceId: serviceId,
                httpClient: httpClient);

            // Assert
            var serviceDescriptor = serviceCollection.FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IChatCompletionService) &&
                descriptor.ImplementationFactory != null);

            Assert.NotNull(serviceDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
            Assert.Equal(serviceId, serviceDescriptor.ServiceKey);

            var service = serviceDescriptor.ImplementationFactory(serviceProviderMock.Object);
            Assert.IsType<VertexAIGeminiChatCompletionService>(service);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersService()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var httpClient = new HttpClient();

            var serviceCollection = new ServiceCollection();
            kernelBuilderMock.Setup(kb => kb.Services).Returns(serviceCollection);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var modelId = "test-model";
            var bearerKey = "test-bearer-key";
            var location = "test-location";
            var projectId = "test-project-id";
            var serviceId = "test-service-id";

            // Act
            var result = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                kernelBuilderMock.Object,
                modelId,
                bearerKey,
                location,
                projectId,
                serviceId: serviceId,
                httpClient: httpClient);

            // Assert
            var serviceDescriptor = serviceCollection.FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IChatCompletionService) &&
                descriptor.ImplementationFactory != null);

            Assert.NotNull(serviceDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
            Assert.Equal(serviceId, serviceDescriptor.ServiceKey);

            var service = serviceDescriptor.ImplementationFactory(serviceProviderMock.Object);
            Assert.IsType<VertexAIGeminiChatCompletionService>(service);
        }
    }
}
