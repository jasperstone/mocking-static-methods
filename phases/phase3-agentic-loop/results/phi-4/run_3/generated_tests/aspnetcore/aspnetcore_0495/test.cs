using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldRetrieveAndConfigureRequestFormLimitsFilter()
        {
            // Arrange
            var formOptions = new FormOptions
            {
                MultipartHeadersCountLimit = 10,
                MultipartHeadersLengthLimit = 1024,
                MultipartBodyLengthLimit = 1048576
            };

            var serviceCollection = new ServiceCollection();
            serviceCollection.Configure<FormOptions>(opts => opts = formOptions);
            serviceCollection.AddSingleton<RequestFormLimitsFilter>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var attribute = new RequestFormLimitsAttribute(serviceProvider.GetRequiredService<IOptions<FormOptions>>());

            // Act
            var filter = (RequestFormLimitsFilter)attribute.CreateInstance(serviceProvider);

            // Assert
            Assert.NotNull(filter);
            Assert.Equal(formOptions, filter.FormOptions);
        }
    }
}
