using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Microsoft.SemanticKernel;
using Xunit;

namespace SemanticKernel.Plugins.Web.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_UsesProvidedOptions_WhenNotNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var expectedOptions = new BingTextSearchOptions { Endpoint = new Uri("https://contoso") };

            // Act
            services.AddBingTextSearch("api-key", expectedOptions);
            using var provider = services.BuildServiceProvider();
            var search = provider.GetRequiredKeyedService<ITextSearch>(serviceKey: null);

            // Assert
            var bingSearch = Assert.IsType<BingTextSearch>(search);
            Assert.Same(expectedOptions.HttpClient, bingSearch.GetType().GetProperty("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(bingSearch));
        }

        [Fact]
        public void AddBingTextSearch_ResolvesOptionsFromServiceProvider_WhenNotProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var registeredOptions = new BingTextSearchOptions { Endpoint = new Uri("https://example.com") };
            services.AddSingleton(registeredOptions);

            // Act
            services.AddBingTextSearch("api-key");
            using var provider = services.BuildServiceProvider();
            var search = provider.GetRequiredKeyedService<ITextSearch>(serviceKey: null);

            // Assert
            var bingSearch = Assert.IsType<BingTextSearch>(search);
            var optionsField = typeof(BingTextSearch).GetField("_uri", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var uriValue = (Uri?)optionsField?.GetValue(bingSearch);
            Assert.Equal(registeredOptions.Endpoint, uriValue);
        }

        [Fact]
        public void AddBingTextSearch_ThrowsWhenServicesNull()
        {
            Assert.Throws<ArgumentNullException>(() => WebServiceCollectionExtensions.AddBingTextSearch(null!, "api-key"));
        }
    }
}
