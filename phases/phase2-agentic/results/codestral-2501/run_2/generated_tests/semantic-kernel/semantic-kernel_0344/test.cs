using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web;
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
        public void AddBingTextSearch_ShouldRegisterBingTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "testApiKey";
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
        public void AddBingTextSearch_ShouldUseServiceProviderOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "testApiKey";
            var options = new BingTextSearchOptions();
            services.AddSingleton(options);

            // Act
            services.AddBingTextSearch(apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_ShouldRegisterBraveTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "testApiKey";
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
        public void AddBraveTextSearch_ShouldUseServiceProviderOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "testApiKey";
            var options = new BraveTextSearchOptions();
            services.AddSingleton(options);

            // Act
            services.AddBraveTextSearch(apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BraveTextSearch>(textSearch);
        }
    }
}
