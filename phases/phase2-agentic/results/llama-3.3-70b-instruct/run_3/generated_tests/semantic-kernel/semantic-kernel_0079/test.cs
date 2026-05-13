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
            var loggerFactory = new LoggerFactory();
            services.AddSingleton<ILoggerFactory>(loggerFactory);

            // Act
            var azureOpenAIServiceCollectionExtensions = new AzureOpenAIServiceCollectionExtensions();
            azureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(services, "deploymentName", "endpoint", "apiKey");

            // Assert
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService<ILoggerFactory>()).Returns(loggerFactory);
            var loggerFactoryResult = serviceProviderMock.Object.GetService<ILoggerFactory>();
            Assert.Same(loggerFactory, loggerFactoryResult);
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
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService<ILoggerFactory>()).Returns((ILoggerFactory)null);
            var loggerFactoryResult = serviceProviderMock.Object.GetService<ILoggerFactory>();
            Assert.Null(loggerFactoryResult);
        }
    }
}
