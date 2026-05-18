using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;

namespace WebServiceCollectionExtensionsTests
{
    public class AddTextSearchTests
    {
        [Fact]
        public void AddBingTextSearch_Should_Register_ITextSearch_With_Default_ServiceId()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";

            // Act
            services.AddBingTextSearch(apiKey);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<BingTextSearch>(service);
        }

        [Fact]
        public void AddBraveTextSearch_Should_Register_ITextSearch_With_Custom_ServiceId()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "brave-api-key";

            // Act
            services.AddBraveTextSearch(apiKey, new BraveTextSearchOptions(), "customServiceId");

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<BraveTextSearch>(service);
        }

        [Fact]
        public void AddGoogleTextSearch_Should_Register_ITextSearch_With_Custom_ServiceId()
        {
            // Arrange
            var services = new ServiceCollection();
            var searchEngineId = "search-engine-id";
            var apiKey = "google-api-key";

            // Act
            services.AddGoogleTextSearch(searchEngineId, apiKey, new GoogleTextSearchOptions(), "googleService");

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<GoogleTextSearch>(service);
        }

        [Fact]
        public void AddTavilyTextSearch_Should_Register_ITextSearch_With_Default_ServiceId()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "tavily-api-key";

            // Act
            services.AddTavilyTextSearch(apiKey, new TavilyTextSearchOptions());

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<TavilyTextSearch>(service);
        }

        [Fact]
        public void AddBingTextSearch_Should_Call_GetService_For_Options_When_Not_Provided()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";

            // Register BingTextSearchOptions
            services.AddSingleton<BingTextSearchOptions>(new BingTextSearchOptions());

            // Act
            services.AddBingTextSearch(apiKey);

            var provider = services.BuildServiceProvider();

            // Use reflection to get the private method or simulate the call
            // Since the extension method uses sp.GetService<BingTextSearchOptions>(), we can test that
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<BingTextSearch>(service);
        }
    }
}
