using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
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
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_WithApiKeyAndNoOptions_RegistersBingTextSearchWithDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            services.AddSingleton<BingTextSearchOptions>(new BingTextSearchOptions());

            // Act
            services.AddBingTextSearch(apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_WithApiKeyAndOptionsAndServiceId_RegistersBingTextSearchWithServiceId()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var options = new BingTextSearchOptions();
            var serviceId = "serviceId";

            // Act
            services.AddBingTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
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
            Assert.NotNull(textSearch);
            Assert.IsType<BraveTextSearch>(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_WithApiKeyAndNoOptions_RegistersBraveTextSearchWithDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            services.AddSingleton<BraveTextSearchOptions>(new BraveTextSearchOptions());

            // Act
            services.AddBraveTextSearch(apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BraveTextSearch>(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_WithApiKeyAndOptionsAndServiceId_RegistersBraveTextSearchWithServiceId()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var options = new BraveTextSearchOptions();
            var serviceId = "serviceId";

            // Act
            services.AddBraveTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
            Assert.IsType<BraveTextSearch>(textSearch);
        }
    }
}
