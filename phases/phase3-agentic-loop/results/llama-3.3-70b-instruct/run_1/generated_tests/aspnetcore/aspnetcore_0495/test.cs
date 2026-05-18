using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ServiceProvider_ReturnsFilter()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<RequestFormLimitsFilter>()
                .BuildServiceProvider();

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var filter = (RequestFormLimitsFilter)attribute.CreateInstance(serviceProvider);

            // Assert
            Assert.NotNull(filter);
            Assert.Same(attribute.FormOptions, filter.FormOptions);
        }

        [Fact]
        public void CreateInstance_ServiceProvider_ThrowsException_WhenFilterNotRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var attribute = new RequestFormLimitsAttribute();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => attribute.CreateInstance(serviceProvider));
        }
    }
}
