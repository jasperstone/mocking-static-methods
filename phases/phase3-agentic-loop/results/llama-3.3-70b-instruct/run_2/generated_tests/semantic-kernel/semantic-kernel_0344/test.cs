using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Xunit;

namespace SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_RegistersITextSearchInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();

            // Act
            services.AddBingTextSearch(apiKey, options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService(typeof(ITextSearch)) as ITextSearch;
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_UsesProvidedOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();

            // Act
            services.AddBingTextSearch(apiKey, options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService(typeof(ITextSearch)) as ITextSearch;
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_UsesServiceId()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            var serviceId = "test-service-id";

            // Act
            services.AddBingTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService(typeof(ITextSearch), serviceId) as ITextSearch;
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_GetServiceReturnsOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            services.AddSingleton(options);

            // Act
            services.AddBingTextSearch(apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService(typeof(ITextSearch)) as ITextSearch;
            Assert.IsType<BingTextSearch>(textSearch);
        }
    }
}
