using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class FormatFilterAttributeTests
    {
        [Fact]
        public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var attribute = new FormatFilterAttribute();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null!));
        }

        [Fact]
        public void CreateInstance_ReturnsFormatFilter_FromServiceProvider()
        {
            // Arrange
            var expectedFormatFilter = new Mock<FormatFilter>(
                Mock.Of<Microsoft.Extensions.Options.IOptions<MvcOptions>>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>())
                .Object;

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(FormatFilter)))
                .Returns(expectedFormatFilter);

            var attribute = new FormatFilterAttribute();

            // Act
            var filter = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(expectedFormatFilter, filter);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(FormatFilter)), Times.Once);
        }
    }
}
