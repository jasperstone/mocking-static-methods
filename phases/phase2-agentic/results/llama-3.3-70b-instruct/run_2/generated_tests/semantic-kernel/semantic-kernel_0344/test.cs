using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
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
            services.AddSingleton<BingTextSearchOptions>();

            // Act
            services.AddBingTextSearch(apiKey, serviceId: serviceId);

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
            services.AddSingleton<BraveTextSearchOptions>();

            // Act
            services.AddBraveTextSearch(apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }
    }
}
