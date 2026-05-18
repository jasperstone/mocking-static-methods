using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class FormatFilterAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldCallGetRequiredServiceAndReturnFormatFilter()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var expectedFormatFilter = new FormatFilter();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<FormatFilter>())
                .Returns(expectedFormatFilter);

            var attribute = new FormatFilterAttribute();

            // Act
            var result = attribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.Same(expectedFormatFilter, result);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<FormatFilter>(), Times.Once);
        }

        [Fact]
        public void CreateInstance_ShouldThrowArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var attribute = new FormatFilterAttribute();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null));
        }
    }
}
