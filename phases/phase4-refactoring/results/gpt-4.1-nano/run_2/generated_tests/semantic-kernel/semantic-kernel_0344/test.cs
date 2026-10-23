using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.SemanticKernel.Plugins.Web;

namespace WebServiceCollectionExtensionsTests
{
    public class AddBingTextSearchTests
    {
        [Fact]
        public void AddsBingTextSearch_WithNullOptions_CallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a mock BingTextSearchOptions
            var mockOptions = new BingTextSearchOptions();

            // Register the extension method
            services.AddBingTextSearch("testApiKey");

            // Build the provider
            var provider = services.BuildServiceProvider();

            // Create a mock IServiceProvider to track GetService calls
            var mockProvider = new Mock<IServiceProvider>();
            bool getServiceCalled = false;

            mockProvider.Setup(sp => sp.GetService<BingTextSearchOptions>())
                        .Returns(() =>
                        {
                            getServiceCalled = true;
                            return mockOptions;
                        });

            // Find the factory delegate
            var serviceDescriptor = services[0];
            var factory = serviceDescriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Act: invoke the factory with our mock provider
            var textSearch = factory!(mockProvider.Object, null);

            // Assert
            Assert.IsType<BingTextSearch>(textSearch);
            var bingTextSearch = textSearch as BingTextSearch;
            Assert.NotNull(bingTextSearch);
            Assert.Equal("testApiKey", bingTextSearch.ApiKey);
            Assert.True(getServiceCalled, "GetService<BingTextSearchOptions>() was not called");
        }
    }
}
