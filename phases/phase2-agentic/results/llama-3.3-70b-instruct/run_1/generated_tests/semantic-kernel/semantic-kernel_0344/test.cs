using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Xunit;

namespace SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_WithNullOptions_GetServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var options = new BingTextSearchOptions();

            services.AddSingleton(options);

            // Act
            services.AddBingTextSearch(apiKey, null, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_WithOptions_GetServiceIsNotCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var options = new BingTextSearchOptions();

            // Act
            services.AddBingTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_WithNullOptions_GetServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var options = new BraveTextSearchOptions();

            services.AddSingleton(options);

            // Act
            services.AddBraveTextSearch(apiKey, null, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_WithOptions_GetServiceIsNotCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var options = new BraveTextSearchOptions();

            // Act
            services.AddBraveTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }
    }
}
