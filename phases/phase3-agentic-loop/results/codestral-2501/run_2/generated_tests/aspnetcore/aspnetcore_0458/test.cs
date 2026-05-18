using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class FormatFilterAttributeTests
    {
        [Fact]
        public void CreateInstance_ServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var attribute = new FormatFilterAttribute();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null));
        }

        [Fact]
        public void CreateInstance_ServiceProviderProvidesFormatFilter_ReturnsFormatFilter()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFormatFilter = new Mock<FormatFilter>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(FormatFilter))).Returns(mockFormatFilter.Object);

            var attribute = new FormatFilterAttribute();

            // Act
            var result = attribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormatFilter>(result);
        }
    }
}
