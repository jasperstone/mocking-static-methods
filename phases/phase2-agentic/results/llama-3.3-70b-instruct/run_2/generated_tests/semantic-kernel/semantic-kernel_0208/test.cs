using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_GetService_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            services.AddOpenAIChatClient("modelId", new Uri("https://example.com"), "apiKey");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
