using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureOpenAIServiceCollectionExtensionsTests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
        }
    }
}
