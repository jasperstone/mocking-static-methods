using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_WithApiKeyAndOptions_RegistersBingTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var options = new BingTextSearchOptions();

            // Act
            services.AddBingTextSearch(apiKey, options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_WithApiKeyAndNoOptions_RegistersBingTextSearchWithDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";

            // Act
            services.AddBingTextSearch(apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_WithApiKeyAndOptions_RegistersBraveTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var options = new BraveTextSearchOptions();

            // Act
            services.AddBraveTextSearch(apiKey, options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.IsType<BraveTextSearch>(textSearch);
        }

        [Fact]
        public void AddGoogleTextSearch_WithSearchEngineIdAndApiKeyAndOptions_RegistersGoogleTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var searchEngineId = "searchEngineId";
            var apiKey = "apiKey";
            var options = new GoogleTextSearchOptions();

            // Act
            services.AddGoogleTextSearch(searchEngineId, apiKey, options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.IsType<GoogleTextSearch>(textSearch);
        }

        [Fact]
        public void AddTavilyTextSearch_WithApiKeyAndOptions_RegistersTavilyTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var options = new TavilyTextSearchOptions();

            // Act
            services.AddTavilyTextSearch(apiKey, options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.IsType<TavilyTextSearch>(textSearch);
        }

        [Fact]
        public void GetService_WithBingTextSearchOptions_ReturnsBingTextSearchOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<BingTextSearchOptions>();
            var options = new BingTextSearchOptions();
            services.Configure<BingTextSearchOptions>(o => o.Endpoint = "endpoint");

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var bingTextSearchOptions = serviceProvider.GetService<IOptions<BingTextSearchOptions>>();

            // Assert
            Assert.NotNull(bingTextSearchOptions);
            Assert.Equal("endpoint", bingTextSearchOptions.Value.Endpoint);
        }
    }
}
