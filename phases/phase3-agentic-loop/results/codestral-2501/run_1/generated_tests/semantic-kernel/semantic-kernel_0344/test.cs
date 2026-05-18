using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_ShouldRegisterBingTextSearchWithDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";

            // Act
            services.AddBingTextSearch(apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);

            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldRegisterBingTextSearchWithProvidedOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions
            {
                Endpoint = new Uri("https://test-endpoint.com")
            };
            var serviceId = "test-service-id";

            // Act
            services.AddBingTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);

            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldUseServiceProviderToGetOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var mockOptions = new Mock<BingTextSearchOptions>();
            services.AddSingleton(mockOptions.Object);

            // Act
            services.AddBingTextSearch(apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);

            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }
    }
}
