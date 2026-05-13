using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act
            services.AddAzureOpenAIChatClient(
                "deploymentName",
                "https://example-endpoint.com",
                "apiKey",
                httpClient: new HttpClient());

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
