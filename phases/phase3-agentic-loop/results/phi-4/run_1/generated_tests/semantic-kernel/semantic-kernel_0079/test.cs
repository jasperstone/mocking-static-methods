using System;
using System.Collections.Generic;
using System.Linq;
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
            var mockServiceProvider = new Mock<IServiceProvider>();
            var loggerFactory = new LoggerFactory();
            mockServiceProvider
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactory);

            var deploymentName = "deployment";
            var endpoint = "https://example.com";
            var apiKey = "fakeApiKey";

            // Act
            services.AddAzureOpenAIChatClient(deploymentName, endpoint, apiKey);

            // Assert
            var provider = services.BuildServiceProvider();
            mockServiceProvider.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
