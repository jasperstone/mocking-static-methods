using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class FormatFilterAttributeTests
    {
        private class DummyFormatFilter : IFilterMetadata { }

        [Fact]
        public void CreateInstance_ReturnsFormatFilter_FromServiceProvider()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddTransient<FormatFilter, DummyFormatFilter>()
                .BuildServiceProvider();

            var attribute = new FormatFilterAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProvider);

            // Assert
            Assert.IsType<FormatFilter>(result);
        }

        [Fact]
        public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var attribute = new FormatFilterAttribute();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null));
        }
    }
}
