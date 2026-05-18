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
        public void CreateInstance_ReturnsFormatFilter_FromServiceProvider()
        {
            // Arrange
            var mockFormatFilter = new Mock<IFilterMetadata>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<FormatFilter>())
                .Returns(new FormatFilter());

            var attribute = new FormatFilterAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.IsType<FormatFilter>(result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<FormatFilter>(), Times.Once);
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

    // Dummy class to satisfy the return type
    public class FormatFilter : IFilterMetadata
    {
    }
}
