using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_ShouldRegisterBingTextSearch()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();

            // Act
            serviceCollection.AddBingTextSearch(apiKey, options);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldUseServiceProviderOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            serviceCollection.AddSingleton(options);

            // Act
            serviceCollection.AddBingTextSearch(apiKey);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldThrow_WhenServicesIsNull()
        {
            // Arrange
            IServiceCollection services = null;
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddBingTextSearch(apiKey, options));
        }
    }
}
