using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_RegistersBingTextSearchInstance()
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
        public void AddBingTextSearch_GetServiceIsCalledWhenOptionsIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            services.AddTransient<BingTextSearchOptions>();

            // Act
            services.AddBingTextSearch(apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.IsType<BingTextSearch>(textSearch);
        }
    }
}
