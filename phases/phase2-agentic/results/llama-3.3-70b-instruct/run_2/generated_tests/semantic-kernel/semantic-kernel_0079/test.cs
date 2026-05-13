using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);

            // Act
            var azureOpenAIServiceCollectionExtensions = new AzureOpenAIServiceCollectionExtensions();
            azureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(services, "deploymentName", "endpoint", "apiKey");

            // Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddAzureOpenAIChatClient_ServiceProvider_GetService_ReturnsNullLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var azureOpenAIServiceCollectionExtensions = new AzureOpenAIServiceCollectionExtensions();
            azureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(services, "deploymentName", "endpoint", "apiKey");

            // Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.Null(loggerFactory);
        }
    }
}
