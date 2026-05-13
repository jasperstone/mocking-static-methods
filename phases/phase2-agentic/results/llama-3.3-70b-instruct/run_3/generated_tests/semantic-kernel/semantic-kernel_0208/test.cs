using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            var services = new ServiceCollection();
            services.AddOpenAIChatClient("modelId", new Uri("https://api.openai.com"), "apiKey");

            // Assert
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOpenAIChatClient_GetService_ThrowsException_WhenLoggerFactoryIsNull()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<ILoggerFactory>());
        }
    }
}
