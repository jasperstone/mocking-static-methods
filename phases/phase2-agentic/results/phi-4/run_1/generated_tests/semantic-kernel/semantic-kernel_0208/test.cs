using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            var modelId = "gpt-3.5-turbo";
            var endpoint = new Uri("https://api.openai.com/v1/chat/completions");
            var apiKey = "test-api-key";
            var orgId = "test-org-id";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddOpenAIChatClient(modelId, endpoint, apiKey, orgId, serviceId, httpClient);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
